# DoctorBooking.DDD.Application.Tests

Модульные тесты для Application Layer (CQRS handlers).

## Структура

```
DoctorBooking.DDD.Application.Tests/
├── Fakes/                           # Fake-реализации для тестирования
│   ├── FakeUnitOfWork.cs           # In-memory Unit of Work
│   ├── FakeUserRepository.cs       # In-memory репозиторий пользователей
│   ├── FakeScheduleRepository.cs   # In-memory репозиторий расписаний
│   ├── FakeAppointmentRepository.cs # In-memory репозиторий записей
│   └── FakeClock.cs                # Управляемые часы для тестов
│
├── Users/
│   └── Commands/
│       ├── RegisterUserHandlerTests.cs
│       └── AddUserRoleHandlerTests.cs
│
├── Schedules/
│   └── Commands/
│       ├── CreateScheduleHandlerTests.cs
│       ├── AddSlotHandlerTests.cs
│       └── RemoveSlotHandlerTests.cs
│
└── Appointments/
    └── Commands/
        ├── BookAppointmentHandlerTests.cs
        ├── CancelAppointmentHandlerTests.cs
        ├── AddPaymentHandlerTests.cs
        ├── CompleteAppointmentHandlerTests.cs
        └── MarkNoShowHandlerTests.cs
```

## Подход к тестированию

### Что тестируем

Application handlers - **тонкий слой оркестрации**, который:
1. Загружает агрегаты/данные из репозиториев
2. Вызывает методы домена (где сосредоточена бизнес-логика)
3. Сохраняет изменения через Unit of Work
4. Возвращает результат (`Result<T>` или `Result`)

### Что НЕ тестируем

- **Бизнес-логику домена** (тестируется в `DoctorBooking.DDD.Domain.Tests`)
- **Инфраструктуру** (EF Core mappings, HTTP controllers)
- **Валидацию FluentValidation** (выполняется через Mediator pipeline behaviors)

### Стратегия

Используем **Fake-реализации** вместо моков (NSubstitute):
- Проще в написании и чтении
- Быстрее выполняются
- Переиспользуются между тестами
- In-memory хранилище позволяет проверять состояние после операций

**Когда использовать моки**: только для внешних зависимостей (email-сервисы, внешние API), которых нет в текущих handlers.

## Паттерн AAA (Arrange-Act-Assert)

Все тесты следуют стандартному паттерну:

```csharp
[Fact]
public async Task Handle_ValidCommand_CreatesEntity()
{
   // Arrange - подготовка данных и зависимостей
   var user = CreatePatient();
   var command = new RegisterUserCommand(...);

   // Act - выполнение тестируемого действия
   var result = await _handler.Handle(command, CancellationToken.None);

   // Assert - проверка результата
   Assert.True(result.IsSuccess);
   Assert.NotEqual(Guid.Empty, result.Value);
}
```

## Ключевые сценарии

Для каждого handler тестируются:

### 1. Happy Path (успешный сценарий)
```csharp
[Fact]
public async Task Handle_ValidCommand_SucceedsAndCallsSaveChanges()
```

### 2. Проверка бизнес-правил домена
```csharp
[Fact]
public async Task Handle_DuplicateEmail_ReturnsFailure()

[Fact]
public async Task Handle_PatientTriesToBookOwnSlot_ThrowsDomainException()
```

### 3. Проверка вызова `SaveChangesAsync`
```csharp
Assert.Equal(1, _uow.SaveChangesCallCount);
```

### 4. Проверка возвращаемого результата
```csharp
Assert.True(result.IsSuccess);
Assert.Equal(expectedId, result.Value);
```

### 5. Граничные случаи
```csharp
[Fact]
public async Task Handle_EntityNotFound_ReturnsFailure()

[Fact]
public async Task Handle_InvalidState_ThrowsDomainException()
```

## Примеры

### RegisterUserHandler

- Успешная регистрация нового пользователя
- Проверка дублирования email (возвращает `Result.Failure`)
- Регистрация с разными ролями (Patient, Doctor, Admin)
- Проверка вызова `SaveChangesAsync`

### BookAppointmentHandler

- Успешное бронирование слота
- Бесплатный слот → автоматическое подтверждение
- Пациент не найден → `DomainException`
- Слот не найден → `SlotNotFoundException`
- Пациент пытается забронировать свой слот → `DomainException`
- Слот в прошлом → `DomainException`
- Слот уже подтвержден → `DomainException`

### CancelAppointmentHandler

- Успешная отмена записи
- Отмена с возвратом денег (>2 часа до встречи)
- Отмена без возврата (<2 часа до встречи)
- Запись не найдена → `Result.Failure`
- Попытка отменить после начала встречи → `DomainException`
- Попытка отменить уже отмененную → `DomainException`

### AddPaymentHandler

- Успешное добавление платежа
- Полная оплата → переход в `Confirmed`
- Частичная оплата → остается `Planned`
- Несколько частичных платежей → итоговое подтверждение
- Превышение суммы → `DomainException`
- Платеж для подтвержденной записи → `InvalidOperationException`
- Отрицательная/нулевая сумма → `DomainException`

### CompleteAppointmentHandler

- Успешное завершение подтвержденной записи
- Запись не найдена → `Result.Failure`
- Попытка завершить неподтвержденную запись → `DomainException`
- Попытка завершить отмененную запись → `DomainException`
- Регистрация события `AppointmentCompleted`

### MarkNoShowHandler

- Успешная отметка неявки для подтвержденной записи
- Запись не найдена → `Result.Failure`
- Попытка отметить неявку неподтвержденной записи → `DomainException`
- Попытка отметить неявку уже завершенной записи → `DomainException`
- Регистрация события `AppointmentNoShow`

### AddSlotHandler

- Успешное добавление слота в расписание
- Расписание не найдено → `Result.Failure`
- Слот в прошлом → `DomainException`
- Пересекающийся слот → `DomainException`
- Создание бесплатного слота (price = 0)
- Отрицательная цена → `DomainException`

### RemoveSlotHandler

- Успешное удаление слота без активных записей
- Расписание не найдено → `Result.Failure`
- Слот с активной записью (Planned/Confirmed) → `DomainException`
- Слот с завершенной записью → удаление разрешено
- Слот с отмененной записью → удаление разрешено
- Удаление одного слота из нескольких

## Запуск тестов

```bash
# Все тесты
dotnet test

# Конкретный проект
dotnet test DoctorBooking.DDD.Application.Tests

# С детализацией
dotnet test --logger "console;verbosity=detailed"

# Только Application.Tests в solution
dotnet test --filter FullyQualifiedName~DoctorBooking.DDD.Application.Tests
```

## Зависимости

- **xUnit** - фреймворк тестирования
- **NSubstitute** - доступен, но не используется (предпочитаем Fakes)
- **DoctorBooking.DDD.Application** - тестируемый проект
- **DoctorBooking.DDD.Domain** - доменные модели и исключения

## Принципы DDD в тестах

1. **Тесты handlers НЕ дублируют тесты домена**  
   Handler проверяет оркестрацию; домен проверяет инварианты.

2. **Handlers вызывают методы агрегатов**  
   Вся бизнес-логика внутри агрегатов, handlers только координируют.

3. **Используем доменные исключения**  
   `DomainException`, `SlotNotFoundException` и т.д.

4. **Результаты через `Result<T>`**  
   Для ожидаемых ошибок используем `Result.Failure`, для неожиданных - исключения.

5. **Проверяем доменные события** (где актуально)  
   ```csharp
   var events = appointment.PopDomainEvents();
   var cancelledEvent = events.OfType<AppointmentCancelled>().FirstOrDefault();
   Assert.NotNull(cancelledEvent);
   Assert.True(cancelledEvent.ShouldRefund);
   ```
