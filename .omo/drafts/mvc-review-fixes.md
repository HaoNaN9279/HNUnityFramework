# mvc-review-fixes — Draft

status: awaiting-approval

## Components

1. **GameWorld Initialize fix (P0)** — `GameWorld.Initialize()` calls `Tick()` instead of `Initialize()`
2. **View binding infrastructure (P1)** — `IReadOnlyModel<T>` + `PropertyBinder` binding API
3. **Virtual hooks (P2)** — `OnInitialize`/`OnClear` in Controller + Model
4. **Code quality (P3)** — foreach→for, PooledList in ControllerManager, naming consistency
5. **Tests** — MVC unit tests in existing test assembly
6. **Docs sync** — architecture, guide, API, code-standard, view-factory

## Decisions made

### D1: IReadOnlyModel<T> placement
- **Decision**: Define `IReadOnlyModel<T>` in Core layer (`HN.Framework.Core.Level.Logic`)
- **Why**: It's a pure C# data contract — no Unity dependency. View layer (Unity) can reference it through assembly reference.

### D2: PropertyBinder binding API shape
- **Decision**: `PropertyBinder.Bind<T>(IReadOnlyModel<T> source, Action<T> onValueChanged)` as abstract method
- **Why**: Minimal API surface. Action-based callback avoids forcing a specific reactive framework.

### D3: Controller-Model association
- **Decision**: No automatic association. Controller holds Model as private field and exposes selected data via IReadOnlyModel properties.
- **Why**: Explicit ownership. No second global registry.

### D4: Naming convention resolution
- **Decision**: Unify to plain camelCase for private fields (per AGENTS.md "私有字段: camelCase").
- **Why**: Controller.cs and Model.cs already use plain camelCase (`controllerUnits`, `modelUnits`). ControllerManager.cs is the outlier using `m_` prefix (`m_controllers`) — change it to `controllers` to match. ProcedureManager.cs also uses `m_` but is outside MVC scope.
- **Files affected**: ControllerManager.cs (`m_controllers` → `controllers`). Controller.cs and Model.cs are already correct.

### D5: Test assembly
- **Decision**: Add tests to existing `HN.Framework.Unity.Tests` assembly.
- **Why**: Already references Core. MVC is pure Core code.

### D6: foreach→for in Initialize/OnFirstFrame
- **Decision**: Change to for loops.
- **Why**: Consistency with Tick/LateTick.

## Pending action
Write `.omo/plans/mvc-review-fixes.md` — **DONE** (plan file already written, awaiting Metis review findings)

## Approach summary
1. Wave 1 (parallel 6 tasks): Fix GameWorld bug, create IReadOnlyModel, refactor Controller/Model/ControllerManager, implement PropertyBinder
2. Wave 2: Write MVC unit tests
3. Wave 3 (parallel 2 tasks): Sync all docs

## Metis review
Status: running (bg_46379848)
