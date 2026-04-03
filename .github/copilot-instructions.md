# Copilot instructions for this repository

## 1. Response Format

- Always reference files and exact line numbers when suggesting changes:
  - Example: `[src/components/MyComponent.vue | 42]`
- Use Markdown only for code blocks and tables.
- Avoid emojis, non-ASCII Unicode characters, and long dashes in output.

## 2. Code Guidelines

- Prefer code that is reliable, readable, and maintainable.
- When showing edits, include only the minimal lines necessary to implement the change.
- Keep explanations short and purposeful - show the code, then a brief reason if needed.
- In Markdown files always use the short table separator syntax: `--- | ---` instead of long dash lines like `| ----------- | ------- |`
- For fields, use the following conventions:
  - `long fields` → required `string` (no initializer)
  - `long? fields` → `string?` (no initializer)
- Query/command/request types should use `record`, not `class`.


## 3. File & Project References

- Respect the repository's working directory and file organization.
- Follow existing project conventions where possible (naming, style, structure).

## 4. Priorities

1. Correctness over cleverness
2. Clarity over brevity
3. Provide explicit steps and make assumptions explicit if any

## 5. Do Not

- Do not guess project conventions without confirming.
- Do not use emojis, special Unicode, or shorthand.
- Do not change unrelated code or formatting in a patch.
