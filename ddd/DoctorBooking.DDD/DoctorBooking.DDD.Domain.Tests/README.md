# DoctorBooking.DDD.Domain.Tests

Модульные тесты для Domain Layer (агрегаты, entities, value objects, domain services).

## Структура

```text
DoctorBooking.DDD.Domain.Tests/
├── Fakes/                           # Fake-реализации для тестирования
│   ├── FakeClock.cs                # Управляемые часы для тестов
│   ├── FakeUserRepository.cs       # In-memory репозиторий пользователей
│   ├── FakeScheduleRepository.cs   # In-memory репозиторий расписаний
│   └── FakeAppointmentRepository.cs # In-memory репозиторий записей
│
├── ValueObjects/
│   ├── EmailTests.cs               # Тесты value object Email
│   └── MoneyTests.cs               # Тесты value object Money
│
├── Users/
│   └── UserTests.cs                # Тесты агрегата UserAgg
│
├── Schedules/
│   └── ScheduleTests.cs            # Тесты агрегата ScheduleAgg
│
├── Appointments/
│   └── AppointmentTests.cs         # Тесты агрегата AppointmentAgg
│
└── Services/
    └── AppointmentBookingServiceTests.cs  # Тесты domain service
```

## Подход к тестированию

### Что тестируем

Domain Layer - **ядро бизнес-логики**, где:
1. **Агрегаты** защищают инварианты и управляют состоянием
2. **Value Objects** обеспечивают типобезопасность и валидацию
3. **Domain Services** реализуют логику, требующую нескольких агрегатов
4. **Domain Events** регистрируются при изменении состояния

### Что НЕ тестируем

- **Инфраструктуру** (EF Core, HTTP, БД)
- **Application handlers** (тестируются в `DoctorBooking.DDD.Application.Tests`)
- **UI/Controllers** (тестируются на интеграционном уровне)

### Стратегия

**Тестируем бизнес-правила и инварианты напрямую:**
- Создаём агрегаты/value objects через конструкторы
- Вызываем методы домена
- Проверяем состояние, выброшенные исключения, зарегистрированные события

**Используем Fake-реализации** для репозиториев в Domain Services:
- Простые in-memory хранилища
- Не требуют моков - реальное поведение
- Переиспользуются между тестами

**Domain Services** требуют репозитории, т.к. работают с несколькими агрегатами.

## Паттерн AAA (Arrange-Act-Assert)

Все тесты следуют стандартному паттерну:

```csharp
[Fact]
public void AddPayment_FullAmount_TransitionsToConfirmed()
{
   // Arrange - подготовка данных
   var appointment = CreatePlanned(price: new Money(100));

   // Act - выполнение тестируемого действия
   appointment.AddPayment(new Money(100), DateTime.UtcNow);

   // Assert - проверка результата
   Assert.Equal(AppointmentStatus.Confirmed, appointment.Status);
}
```

## Ключевые сценарии

Для каждого агрегата/value object тестируются:

### 1. Валидация конструктора
```csharp
[Fact]
public void Constructor_InvalidData_ThrowsDomainException()
```

### 2. Защита инвариантов
```csharp
[Fact]
public void RemoveRole_LastRole_ThrowsDomainException()
```

### 3. Переходы состояния
```csharp
[Fact]
public void AddPayment_FullAmount_TransitionsToConfirmed()
```

### 4. Регистрация доменных событий
```csharp
[Fact]
public void AddRole_NewRole_RegistersUserRoleAddedEvent()
```

### 5. Value object equality и immutability
```csharp
[Fact]
public void Equality_SameValue_Equal()
```

## Примеры

### AppointmentAgg

**Constructor**
- Валидация: пациент не может быть своим врачом → `DomainException`
- Начальный статус - `Planned`
- Регистрация события `AppointmentCreated`

**AddPayment**
- Успешное добавление частичного платежа
- Полная оплата → переход в `Confirmed`
- Превышение суммы → `DomainException`
- Платеж для подтвержденной записи → `InvalidOperationException`
- Регистрация `PaymentAdded` и `AppointmentConfirmed` событий

**ConfirmFree**
- Подтверждение бесплатного слота (price = 0)
- Попытка подтвердить платный слот → `InvalidOperationException`

**Cancel**
- Успешная отмена до начала встречи
- Отмена после начала → `DomainException`
- Возврат денег (>2 часа до встречи)
- Без возврата (<2 часа до встречи)

**Complete / MarkNoShow**
- Только для подтвержденных записей
- Регистрация соответствующих событий

### UserAgg

**Constructor**
- По умолчанию роль `Patient`
- Валидация Email и PersonName (через value objects)
- Регистрация `UserRegistered`

**AddRole / RemoveRole**
- Добавление новой роли → событие `UserRoleAdded`
- Удаление роли → событие `UserRoleRemoved`
- Идемпотентность при добавлении существующей роли
- Защита: нельзя удалить последнюю роль → `DomainException`

**IsDoctor / IsPatient / IsAdmin**
- Проверка наличия конкретных ролей

### ScheduleAgg

**AddSlot**
- Добавление слота в будущее время
- Слот в прошлом → `DomainException`
- Пересечение слотов → `DomainException`
- Регистрация `SlotAdded`

**RemoveSlot**
- Успешное удаление существующего слота
- Несуществующий слот → `DomainException`
- Регистрация `SlotRemoved`

### Email (Value Object)

- Валидация формата email
- Нормализация (lowercase, trim)
- Пустая строка → `ArgumentException`
- Невалидный формат → `ArgumentException`
- Equality по значению
- Immutability

### Money (Value Object)

- Отрицательная сумма → `ArgumentException`
- Zero - специальный экземпляр
- Сложение и вычитание
- Вычитание с отрицательным результатом → `InvalidOperationException`
- Операторы сравнения (>, <, >=, <=)
- Equality и immutability

### AppointmentBookingService (Domain Service)

**Book**
- Успешное бронирование слота пациентом
- Пациент не найден → `DomainException`
- Слот не найден → `SlotNotFoundException`
- Пациент не имеет роли Patient → `DomainException`
- Пациент пытается забронировать свой слот → `DomainException`
- Слот в прошлом → `DomainException`
- Слот уже подтвержден → `DomainException`
- Бесплатный слот → автоматическое подтверждение

## Запуск тестов

```bash
# Все тесты
dotnet test

# Конкретный проект
dotnet test DoctorBooking.DDD.Domain.Tests

# С детализацией
dotnet test --logger "console;verbosity=detailed"

# Только Domain.Tests в solution
dotnet test --filter FullyQualifiedName~DoctorBooking.DDD.Domain.Tests
```

## Зависимости

- **xUnit** - фреймворк тестирования
- **DoctorBooking.DDD.Domain** - тестируемый проект
- **Core.Common.Domain** - базовые типы (AggregateRoot, Entity, ValueObject, DomainEvent)

## Принципы DDD в тестах

1. **Агрегаты защищают инварианты**
   Тесты проверяют, что инварианты не могут быть нарушены ни при каких обстоятельствах.

2. **Доменные события регистрируются, но не публикуются**
   ```csharp
   var events = aggregate.PopDomainEvents();
   var createdEvent = Assert.IsType<AppointmentCreated>(events[0]);
   ```

3. **Value Objects - immutable и equality по значению**
   ```csharp
   Assert.Equal(new Email("a@b.com"), new Email("A@B.COM"));
   ```

4. **Состояние управляется только методами агрегата**
   Нет прямых сеттеров - только методы с явной бизнес-семантикой.

5. **Domain Services для логики между агрегатами**
   `AppointmentBookingService` координирует Schedule, User и Appointment.

## Паттерны тестирования

### Тестирование агрегатов

```csharp
// 1. Создание через конструктор
var appointment = new AppointmentAgg(id, slotId, patientId, doctorId, start, price);

// 2. Вызов метода домена
appointment.AddPayment(new Money(50), DateTime.UtcNow);

// 3. Проверка состояния
Assert.Equal(new Money(50), appointment.PaidTotal());
Assert.Equal(AppointmentStatus.Planned, appointment.Status);

// 4. Проверка событий
var events = appointment.PopDomainEvents();
var paymentEvent = Assert.IsType<PaymentAdded>(events[0]);
```

### Тестирование Value Objects

```csharp
// 1. Валидация через конструктор
var exception = Assert.Throws<ArgumentException>(() => new Money(-10));

// 2. Операции возвращают новый экземпляр (immutability)
var m1 = new Money(100);
var m2 = m1 + new Money(50);
Assert.Equal(new Money(100), m1); // m1 не изменился
Assert.Equal(new Money(150), m2);

// 3. Equality по значению
Assert.Equal(new Money(100), new Money(100));
```

### Тестирование Domain Services

```csharp
// 1. Подготовка: создать агрегаты и сохранить в fake-репозитории
var patient = new UserAgg(...);
_userRepo.Save(patient);

var schedule = new ScheduleAgg(...);
var slot = schedule.AddSlot(...);
_scheduleRepo.Save(schedule);

// 2. Вызов domain service
var appointment = _bookingService.Book(patient.Id, slot.Id);

// 3. Проверка результата
Assert.NotNull(appointment);
Assert.Equal(AppointmentStatus.Planned, appointment.Status);

// 4. Проверка, что агрегат сохранен в репозитории
var saved = _appointmentRepo.FindById(appointment.Id);
Assert.NotNull(saved);
```
