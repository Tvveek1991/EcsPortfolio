# EcsTestProject

Небольшая 2D-платформенная игра на Unity, созданная как учебный проект для
экспериментов с гибридной ECS-архитектурой. Игровое состояние и правила
реализованы на **LeoEcsLite**, а Unity отвечает за физику, отображение,
анимации и пользовательский интерфейс.

## Возможности

- управление персонажем: бег, прыжок, перекат, атака и блок;
- разрушаемые объекты, здоровье, урон и смерть;
- сбор монет, счётчик и анимация подбора;
- камера, UI, анимации персонажа и эффекты частиц;
- безопасный перезапуск игровой сессии.

## Технологии

| Область | Используемое решение |
| --- | --- |
| Движок | Unity `6000.3.25f1` |
| ECS | LeoEcsLite |
| DI | VContainer |
| Асинхронные операции | UniTask |
| Загрузка ассетов | Unity Addressables |
| Ввод | Unity Input System |
| Реактивные подписки | UniRx |
| Анимация | DOTween |
| Рендеринг | Universal Render Pipeline (URP) |

Версии пакетов закреплены в [Packages/manifest.json](Packages/manifest.json),
а точные разрешённые зависимости — в `Packages/packages-lock.json`.

## Быстрый старт

1. Установите Unity Hub и Unity Editor версии `6000.3.25f1`.
2. Клонируйте репозиторий:

   ```bash
   git clone <URL-репозитория>
   ```

3. В Unity Hub выберите **Add project from disk** и укажите корень этого
   репозитория.
4. Дождитесь восстановления пакетов и импорта ассетов.
5. Откройте сцену [Main.unity](Assets/Project/Scenes/Main.unity) и нажмите
   **Play**.

Не удаляйте `Library/` вручную во время открытого проекта: Unity сможет
пересобрать её, но повторный импорт займёт заметное время.

## Управление

| Действие | Клавиатура и мышь | Геймпад |
| --- | --- | --- |
| Движение | `A` / `D` | Левый стик |
| Прыжок | `Space` | Нижняя лицевая кнопка |
| Перекат | `Left Shift` | Нажатие левого стика |
| Атака | Левая кнопка мыши | Левая лицевая кнопка |
| Блок | Правая кнопка мыши | Левый триггер |
| Тестовый урон | `Q` | — |
| Тестовая смерть | `E` | — |

Настройки действий находятся в
[GameplayInputActions.inputactions](Assets/Project/Resources/Input/GameplayInputActions.inputactions).

## Устройство проекта

```text
Assets/Project/
├── Scenes/                 # Игровые сцены
├── Prefabs/                # View и UI-префабы
├── Data/                   # ScriptableObject-конфигурация
├── Resources/Input/        # Карта действий Input System
└── Scripts/
    ├── Application/        # Bootstrap, состояния приложения и DI scope
    ├── AssetProvider/      # Загрузка и освобождение Addressables
    ├── Gameplay/
    │   ├── Components/     # ECS-компоненты и команды
    │   ├── Ecs/            # Игровая сессия, циклы и composer систем
    │   ├── Systems/        # ECS-системы по игровым фазам
    │   ├── Services/       # Bridge, реестры view, ввод и tween ownership
    │   └── Views/          # Unity-представления ECS-сущностей
    └── Loading/            # Экран и логика загрузки
Documentation/ECS/          # Архитектура, правила и сценарии проверки
```

## ECS-цикл

Порядок игровых систем объявлен в
[GameSystemsInstaller.cs](Assets/Project/Scripts/Gameplay/GameSystemsInstaller.cs):

```text
Initialization → Input → Simulation → Physics → Presentation → Cleanup
```

`ApplicationState` обновляет ввод и выполняет обычный tick через Unity
`Update`; физическая группа выполняется через `FixedUpdate`. Каждая игровая
сессия владеет собственными `EcsWorld`, набором систем, view-реестром и
отменяемыми операциями. При перезапуске они завершаются в контролируемом
порядке.

Подробности и соглашения:

- [архитектура ECS](Documentation/ECS/architecture.md);
- [правила переходов между фазами](Documentation/ECS/phase-transition-rules.md);
- [план развития](PLAN_ECS.md);
- [шаблон новой ECS-системы](Documentation/ECS/EcsSystemTemplate.cs.txt);
- [checklist для ECS-изменений](Documentation/ECS/pr-checklist.md);
- [сценарий профилирования](Documentation/ECS/profiling-baseline.md).

## Разработка

При добавлении механики:

1. Определите её ECS-фазу, входные и выходные компоненты.
2. Добавьте систему в `GameSystemsInstaller` и явно укажите её порядок в
   `GameSystemsComposer`.
3. Для Unity API, view, tween и асинхронных операций используйте bridge-слой
   и владельца текущей игровой сессии.
4. Укажите срок жизни одноразовых команд: по умолчанию их удаляет
   `EndOfFrameCleanupSystem`.
5. Проверьте сценарий в Play Mode и отсутствие ошибок в Unity Console.

Перед созданием pull request пройдите
[ECS checklist](Documentation/ECS/pr-checklist.md).

## Лицензия

Лицензия для исходного кода пока не объявлена. До её добавления не считайте
проект доступным для повторного использования вне условий владельца
репозитория.
