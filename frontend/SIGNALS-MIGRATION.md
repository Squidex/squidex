# Signals migration conventions

Working notes for migrating Angular components in `frontend/src/app` to signals. Delete this file when the migration is done.

## Goal

Every component uses signal APIs only. When you are done with a component, it contains none of:

- `@Input`, `@Output`, `EventEmitter`
- `@ViewChild`, `@ViewChildren`, `@ContentChild`, `@ContentChildren`
- `@HostBinding`, `@HostListener`
- lifecycle hooks: `ngOnInit`, `ngOnChanges`, `ngOnDestroy`, `ngAfterViewInit`, `ngAfterContentInit`, `ngAfterViewChecked`, `ngDoCheck`, ...
- `extends StatefulComponent` / `extends StatefulControlComponent`

Check with:

```bash
grep -nE '@Input\(|@Output\(|@ViewChild|@ViewChildren|@ContentChild|@HostBinding|@HostListener|new EventEmitter|ng(OnInit|OnChanges|OnDestroy|AfterViewInit|AfterContentInit|AfterViewChecked|DoCheck)\(|extends Stateful' <file>
```

Directives and pipes are out of scope. Do not change `| async` usages of state stores unless the component itself builds the observable (then use `toSignal`).

## Rules

1. **Inputs / outputs**: `input()`, `input.required()`, `output()`, `model()`. Use `{ alias: '...' }` for renamed inputs. Keep `booleanAttribute` / `numberAttribute` transforms. Give boolean inputs a real default (`input(false, { transform: booleanAttribute })`) instead of `?: boolean | null`.
2. **All signal fields are `readonly`**: inputs, outputs, `signal`, `computed`, `linkedSignal`, `viewChild`, `toSignal` results. Plain non-signal fields like `new ModalModel()` or form objects are `readonly` too where they are never reassigned.
3. **Document every input and output** with a one-line `/** ... */` comment above it. Describe what it does for the user of the component, not the type. No other JsDoc and **no new `//` comments at all** (not even to explain `untracked` or timing). Keep comments that already existed.
4. **No lifecycle hooks** in components:
   - derived values -> `computed()`
   - derived state that can also be changed locally (e.g. "reset when input changes, but user can toggle") -> `linkedSignal()`
   - side effects depending on inputs (subscriptions, DOM, 3rd party libs) -> `effect(onCleanup => { ...; onCleanup(() => ...) })` in the constructor
   - one-time DOM work after first render -> `afterNextRender()` in the constructor
   - cleanup that is not tied to a subscription -> `DestroyRef.onDestroy()`
   - **Do not use the `Subscriptions` helper** in components. Remove it when migrating:
     - a subscription that only stores a value -> `toSignal(observable, { initialValue })`
     - a subscription with side effects -> `effect(onCleanup => { const subscription = observable.subscribe(...); onCleanup(() => subscription.unsubscribe()); })`. If the observable depends on inputs, read them inside the effect so it resubscribes when they change.
     - unsubscribe functions (e.g. `renderer.listen`, `resizeService.listen`) -> `effect(onCleanup => { const unlisten = ...; onCleanup(unlisten); })`
   - things that only need injected services (no inputs) -> the constructor
   - `@HostListener('window:x', ...)` -> `host: { '(window:x)': 'handler($event)' }`; `@HostBinding('class.x')` -> `host: { '[class.x]': 'signal()' }`
5. **`computed` always has a block body** with an explicit `return`, even for one-liners:
   ```ts
   public readonly isRightAligned = computed(() => {
       return this.alignment() === 'right';
   });
   ```
6. **Parameterless derived state read by the template must be a `computed`**, not a getter or a method. Methods that take a per-item argument and are called inside `@for` (e.g. `isSelectedQuery(saved)`) stay methods; do not replace them with Sets/maps.
7. **`untracked`** only where it is really needed (e.g. an effect that would otherwise depend on the signal it writes). Never "just in case".
8. **`StatefulComponent` / `StatefulControlComponent`**: replace with plain signals. Form controls (ControlValueAccessor) extend `SignalControlComponent<TValue>` from `@app/framework/internal` (see `framework/angular/signal-control.component.ts`). It provides the `disabled` input, `isDisabled` computed, `callChange`, `callTouched`, `setDisabledState`. Remove the component's own `disabled` input setter.
9. **Change detection**: components that called `changeDetector.detach()` must not do so anymore (effects of detached views never run). Prefer `ChangeDetectionStrategy.OnPush` once all state is signals.
10. **Templates**: call signals (`value()`), use `@let x = signal();` to avoid repeated calls in a block, `@if (x(); as x)` to narrow nullable values. Watch out for `@for` loop variables shadowing component signals of the same name - rename the signal.
11. **Consumers**: other templates may read a component's fields via template references (`#row` -> `row.fieldName`) or `viewChild`; they now need `()`. Fix all consumers. `ng build` warns with `NG8109: x is a function and should be invoked` - treat every NG8109 warning as an error.
12. **Output types**: `output<T>()` is stricter than `EventEmitter<any>`. If a parent handler is narrower than what the component really emits, widen the handler (or make the component generic, like `LanguageSelectorComponent<T>`), do not cast.
13. **Naming**: related variables share a prefix (`valueParsed`, `valueForeground`; `tagValuesSorted`, `tagValuesUnsorted`). No conditions spanning multiple lines - split into early returns. No blank line between an assignment and the `if` that checks it. Files under 300 lines where reasonable.
14. **Behavior**: keep behavior the same. If you find a real bug while migrating (e.g. a wrong attribute transform), fix it only if it is obvious and small, and report it.
15. **Specs**: specs that create components with `new` must switch to `TestBed.createComponent` + `fixture.componentRef.setInput(...)` + `fixture.detectChanges()`. Keep `verify`/`expect` calls next to each other without blank lines between; helper functions go after all tests.

## Reference implementations (already migrated)

- Simple inputs/outputs: `framework/angular/pager.component.ts`, `shared/components/contents/content-status.component.ts`
- `linkedSignal`: `shared/components/table-header.component.ts`, `framework/angular/forms/form-error.component.ts`
- `effect` + `onCleanup`: `framework/angular/shortcut.component.ts`, `framework/angular/forms/control-errors.component.ts`
- `afterNextRender` + `viewChild`: `framework/angular/modals/modal-dialog.component.ts`, `framework/angular/menu.component.ts`
- Form control: `framework/angular/forms/editors/date-time-editor.component.ts`, `framework/angular/forms/editors/checkbox-group.component.ts`
- `toSignal` from input-driven observable: `shared/components/assets/asset-history.component.ts`
- Host listener: `shared/components/cards/iframe-card.component.ts`

## Verification

Run from `frontend/`:

```bash
npx ng build --configuration development --output-path <your-own-temp-dir>
```

```bash
npx eslint <changed .ts files>
```

```bash
npx ng test --no-watch
```

Other agents may be editing other folders at the same time, so errors in files outside your scope can be transient. Only fix errors in your own files and in consumers of your components. If a consumer you need to fix is in another agent's folder, do not edit it; report it instead.

---

# Phase 2: states, services and template-only members

## What already changed (do not redo)

- `framework/state.ts`: `State<T>` is signal-based. `project`, `projectFrom`, `projectFrom2` now return `Signal<M>` (a `computed`), and `projectFrom*` take signals instead of observables. `state.value` is a signal of the whole snapshot. `state.snapshot` (untracked read) and `state.changes` (observable of `{ snapshot, event }`) still exist.
- `framework/utils/modal-view.ts`: `ModalModel.isOpen` / `DialogModel.isOpen` are signals (`isOpen()`). `isOpenChanges` is still an observable for the modal directive.
- `StatefulComponent` was deleted.
- `Form.submitCount`, `submitted`, `submitting`, `error` (framework/angular/forms/model.ts) are signals now because they use `project`.

Because of this, the build is broken everywhere until all areas are migrated. Other agents work in parallel on other folders.

## Rules

1. **State stores** (`*.state.ts`, `table-settings.ts`, other classes extending `State<T>`): every public projection is a `Signal`. Remove observables that only exist to project state; derive with `project`/`projectFrom`/`computed`. Keep method signatures that return `Observable` for actions (`load()`, `create()`, ...) – HTTP and actions stay observables, the caller subscribes.
   - Projection names: drop a `Changes` or `$` suffix if the plain name is free (`itemsChanges` -> `items`), otherwise keep the name.
   - Getters that read `snapshot` (`appName`, `appId`) stay as they are.
2. **Services**:
   - **Values with a current state** (`BehaviorSubject`, `ReplaySubject(1)` holding "the current X"): expose a readonly signal (`private readonly x = signal(...)`, `public readonly xValue = this.x.asReadonly()` or a `computed`). Remove the observable if nobody needs it as a stream anymore; if a consumer really needs a stream (e.g. `switchMap` to HTTP), it uses `toObservable(signal)` in an injection context.
   - **Event streams** (`Subject` without a current value: notifications, dialog requests, message bus, tour events) stay observables.
   - **HTTP services** (`*.service.ts` calling `HttpClient`) stay observable-based. Do not use `httpResource` / `resource`.
3. **Consumers**:
   - Templates: `state.selectedApp | async` -> `state.selectedApp()`. Use `@let x = state.selectedApp();` or `@if (state.selectedApp(); as app)` instead of `| async` in `@if`. Remove `AsyncPipe` from `imports` when no `| async` is left.
   - TypeScript: `toSignal(state.x.pipe(...))` -> `computed(() => { ... })`; `state.x.pipe(defined())` -> `computed` that returns the value (use `!`/a non-null computed only if the route guarantees it, same as before with `defined()`). Subscriptions on projections with side effects -> `effect`. If a real stream is needed (`switchMap` to HTTP, `take(1)` in a guard/resolver), use `toObservable(state.x)` – inside an injection context only (constructor, field initializer, guard/resolver function).
   - `| async` on things that are not state projections (HTTP observables, event streams) stays.
4. **Specs**: `state.x.subscribe(result => { actual = result; }); expect(actual)...` -> `expect(state.x())...`. Use `TestBed.runInInjectionContext` / `TestBed.inject` only if something needs an injection context.
5. **Template-only members are `protected`**: in every component (and directive) in your area, every member (field, signal, method, injected constructor parameter like `public readonly appsState: AppsState`) that is only used by its own template becomes `protected`. Members only used inside the class become `private`. Stay `public`:
   - inputs, outputs, models (the component API);
   - members used from outside: other templates via template references (`#row` -> `row.fieldName()`), `viewChild` in parents, other classes, routes, specs, stories (`*.stories.ts`). Grep for the member name before you narrow it.
   - Do not make a member `private` if the template uses it (template type-check fails).
6. Everything from phase 1 still applies: readonly signals, block-body computeds, no lifecycle hooks, no `Subscriptions`, no new comments, `untracked` only when needed, `/** */` only on inputs/outputs.

## Verification for phase 2

While other areas are still in progress the global build has errors outside your area. Filter the output of `npx ng build --configuration development --output-path <your-own-temp-dir>` (or `npx tsc -p tsconfig.app.json --noEmit` and `npx tsc -p tsconfig.spec.json --noEmit`) to your own folders and get them to zero errors and zero NG8109 warnings. Run `npx eslint` on your changed files and `npx ng test --no-watch` filtered to your specs (`npx ng test --no-watch --include <glob>`), if possible.
