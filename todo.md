# Ten feature requests worth implementing next

Selected from the **195 not-implemented feature requests** extracted from
<https://support.squidex.io> (see [FEATURE-REQUESTS.md](FEATURE-REQUESTS.md) for
all 588). Every item was checked against the working copy at `D:\Squidex`
(branch `stability`, HEAD `fc026a3`, v7.23.0) before being listed.

**This revision:** self-service password reset was dropped — with an external
identity provider in most deployments it is dead weight. The list is also
weighted towards substantial features, and every *declined* and *postponed*
candidate was re-read against today's code rather than taken at face value.

## Why re-reading the declines mattered

Most declines date from 2019–2022 and several rest on constraints the codebase
no longer has. Three of the ten below are recommended **because the maintainer's
own stated blocker has since been removed:**

| Then | Now |
| --- | --- |
| #5286: *"we are only dealing with strings at the moment. So a solution could be to save the remirror state tree instead of html or string"* (2023) | Exactly that shipped — *"New rich text field that stores the content as JSON structure"* (`CHANGELOG.md:300`), with `RichTextNode`/`RichTextMark`/`SquidexRichText` in the domain |
| #1366/#3461: UI export is *"a lot of work with very little benefit"*, *"not possible"* (2019–2021) | A generic job framework exists — `IJobRunner` with `RunAsync` **and `DownloadAsync(job, stream, ct)`**, described as unifying *"rules, backups and future background jobs"* (`CHANGELOG.md:266`) |
| #1229/#5676: asset permissions are *"super complicated"* (2019–2024) | Per-schema scoping is now proven in the same permission grammar — `contents.{schema}.read`, even `contents.{schema}.read.own` (`PermissionIds.cs:212-215`) |

Conversely, the same exercise **removed** items: #3321 (concrete GraphQL
reference types) and #1852 (markdown editor) have quietly been implemented since
being postponed and declined. See
[Rejected because already covered](#rejected-because-already-covered).

## The list

| # | Feature | Forum status | Views | Size |
| --- | --- | --- | --- | --- |
| [1](#1-visual-diff-between-content-versions-5286) | Visual diff between content versions | Postponed | 1412 | large |
| [2](#2-export-and-import-content-from-the-ui-as-jobs-3461-1366-3438) | Export/import content from the UI as jobs | Declined ×2 | 5381 | large |
| [3](#3-scope-asset-permissions-to-folders-5921-5676-1229) | Scope asset permissions to folders | Open ×2 + Declined | 3857 | large |
| [4](#4-dark-mode-and-per-app-theming-5720-816) | Dark mode and per-app theming | Open + Declined | 2102 | large |
| [5](#5-azure-service-bus-flow-step-4522) | ✅ **Done** — Azure Service Bus flow step | Postponed | 1838 | medium |
| [6](#6-give-scripts-a-way-to-log-2822) | Give scripts a way to log | Unresolved | 2575 | medium |
| [7](#7-upsert-mode-for-the-create-content-flow-step-5616) | ✅ **Done** — Upsert mode for "Create content" step | Unresolved | 786 | small |
| [8](#8-regenerate-a-client-secret-from-the-ui-5807) | ✅ **Done** — Regenerate a client secret from the UI | Open | 1383 | small |
| [9](#9-show-which-fields-are-required-when-publishing-4375) | ✅ **Done** — Mark "required when publishing" fields | Declined | 1707 | tiny |
| [10](#10-let-users-upload-a-duplicate-asset-anyway-3144) | ✅ **Done** — "Upload anyway" for duplicate assets | Declined | 1356 | tiny |

Items 1–4 are projects. 5–6 are a few days each. 7–10 are hours, and are kept
because they are cheap enough that leaving them undone is the expensive choice.

---

## 1. Visual diff between content versions (#5286)

**Asked:** highlight what changed between two versions of a long article,
instead of leaving the reviewer to read both.
<https://support.squidex.io/t/postponed-change-comparison-similar-to-git/5286>
· Postponed · 1412 views · 2023-11

**Why it matters.** Review is where a CMS earns its keep, and it is the one
place Squidex currently hands the work back to the human. The same need arrives
from three directions in the archive: this request,
[#6120](https://support.squidex.io/t/implement-a-way-to-add-suggestions-to-draft-items/6120)
(*"We need to review blog posts before they are published… comments aren't well
suited to review longer blogposts as it's hard to reference the actual
content"*, Open, 2026-03) and
[#4518](https://support.squidex.io/t/very-unlikely-allow-multiple-users-to-view-content-and-only-one-user-to-edit/4518)
(knowing whether someone else is mid-edit). Workflows already move content
through review states; what is missing is seeing *what* is under review.

**Why the decline no longer applies.** This is the clearest changed-circumstance
in the archive. The maintainer's reasoning in 2023 was:

> *"The problem is that we are only dealing with strings at the moment. So a
> solution could be to save the remirror state tree instead of html or string
> and then make the html or markdown conversion on the fly."*

That prerequisite is now shipped. `CHANGELOG.md:300` records a *"New rich text
field that stores the content as JSON structure and provides on demand
formatting into HTML and markdown over GraphQL"*, and the domain model is
there: `RichTextNode`, `RichTextMark`, `RichTextExtensions`, `SquidexRichText`
(`backend/src/Squidex.Domain.Apps.Core.Model/Contents/`). Retrieving the two
versions to compare is already supported —
`ContentsController.GetContent(app, schema, id, long version)` (line 147) takes
a version and passes it to `contentQuery.FindAsync`.

**Scope it narrowly.** The second objection in that thread was that a two-state
diff is ambiguous (*"Hello World"* → *"World is big"* — what changed?) and that
tools like the Yjs demo work by keeping a change history instead. That is an
argument against *perfect attribution*, not against a diff. The requester asked
for the modest version — *"the simplest implementation… would be to add change
highlighting"*. A node-level structural diff over `RichTextNode` (paragraph or
block added / removed / changed, with text-level diff inside a changed block)
answers the actual need and needs no new storage. Full change tracking can
follow the collaborative-editing work
([#5186](https://support.squidex.io/t/collaborative-editing/5186)) if it is ever
wanted.

**Note.** Non-rich-text fields (strings, numbers, references) can be diffed
trivially and are worth doing in the same pass — the existing comparison view
is where this belongs.

---

## 2. Export and import content from the UI as jobs (#3461, #1366, #3438)

**Asked:** export schemas and content to CSV from the UI, and import from a
spreadsheet, without dropping to the CLI.
- <https://support.squidex.io/t/declined-csv-export-in-cloud/3461> · Declined · 1514 views
- <https://support.squidex.io/t/declined-export-schemas-to-csv/1366> · Declined · 1424 views
- <https://support.squidex.io/t/import-content-from-spreadsheet/3438> · Unresolved · 2443 views

**Why it matters.** 5381 views across three threads spanning 2019–2021, and the
maintainer acknowledged the pattern himself: *"it has been requested a few times
now"*. The standing answer is "use the CLI", which does not help the people who
ask — editors and evaluators, not operators. One thread is explicitly a customer
evaluating Squidex *"as a master database"*. Cloud users cannot be pointed at a
CLI-shaped answer as readily as self-hosters, and #3461 is precisely about that.

**Why the declines no longer apply.** Both were refused on cost and
feasibility — *"there are no plans to implement such features in the UI, as it
is a lot of work with very little benefit"* (2020) and *"it is not possible and
also not planned"* (2021). The expensive parts were long-running work, progress
reporting, and delivering a large file. All three are now framework concerns:

- `IJobRunner` (`backend/src/Squidex.Domain.Apps.Entities/Jobs/IJobRunner.cs`)
  is a four-member interface: `Name`, `RunAsync(JobRunContext, ct)`, and
  optional `DownloadAsync(Job, Stream, ct)` / `CleanupAsync`.
- `DownloadAsync` exists *for exactly this shape of job* — `BackupJob` implements
  it to stream an archive back to the user
  (`.../Backup/BackupJob.cs:28,32,54`).
- `JobLogMessage` and `JobProcessor` give per-job progress and logs, which is
  what an importer needs to report which rows failed.
- Six runners already exist to copy from: `BackupJob`, `RestoreJob`,
  `CreateIndexJob`, `DropIndexJob`, `MigrateContentsJob`, `RuleRunnerJob`.

**Split the two halves; they are not equally hard.**

*Export* is now genuinely straightforward: a job that runs the existing content
query, writes CSV/JSON to a stream, and returns it through `DownloadAsync`. The
CLI already contains the field-flattening logic to borrow.

*Import* keeps the one objection that still stands — from #3438: *"Usually the
CSV does not match to the content format. Therefore you need some kind of
mapping and a UI to define this mapping."* That is real, and it is the mapping
UI, not the ingestion. The write path is already there: `BulkUpdateContents`,
`BulkUpdateJob` and `UpsertContent`
(`backend/src/Squidex.Domain.Apps.Entities/Contents/Commands/`). Ship export
first; it is most of the demand and a fraction of the work.

---

## 3. Scope asset permissions to folders (#5921, #5676, #1229)

**Asked:** restrict a role to particular asset folders rather than all-or-nothing
read/write.
- <https://support.squidex.io/t/limit-role-permissions-on-assets-to-specific-folders/5921> · **Open** · 775 views · 2025-05
- <https://support.squidex.io/t/admin-user-roles-for-assets/5676> · **Open** · 1174 views · 2024-09
- <https://support.squidex.io/t/declined-granular-user-permission/1229> · Declined · 1908 views · 2019-10

**Why it matters.** Asked three times over six years, twice still open, 3857
views combined, and always with the same concrete shape: an agency or
multi-tenant install wanting `clients/XYZ` visible only to that client's
editors. The current workaround the maintainer offers — *"a workaround would be
to have a second app"*, *"create multiple apps and automate app management with
the API or CLI"* — fragments content across apps and, as he notes himself in
#5676, *"linking is difficult then"*. This is the largest recurring gap in the
permission model.

**Why the decline deserves another look.** Assets are the only major entity with
no scoping. Content has had per-schema permissions all along, and the grammar
supports finer grain than that:

```
backend/src/Squidex.Shared/PermissionIds.cs:212-215
public const string AppContents        = "squidex.apps.{app}.contents.{schema}";
public const string AppContentsRead    = "squidex.apps.{app}.contents.{schema}.read";
public const string AppContentsReadOwn = "squidex.apps.{app}.contents.{schema}.read.own";
```

Assets, by contrast, are flat (lines 166-178): `assets.read`, `assets.create`,
`assets.upload`, `assets.update`, `assets.delete`, plus `assets.folders.*` for
managing folders themselves. Adding a folder placeholder follows a pattern that
already works, in the same file, with the same matching machinery.

**Decide the two open design questions up front** — they are why this stalled,
and both have a defensible narrow answer:

1. *Folders are renamed and moved, so paths are unstable* (#5921). Scope by
   folder **id**, not path, exactly as content scopes by schema id. The
   maintainer reached the same conclusion in-thread: *"only the folder ID would
   work. You probably just want to associate a folder with a role."*
2. *What happens when a permitted user references an asset that another user
   cannot see?* (#5676: *"user A adds an asset to a field that he is allowed to
   see and user B opens the content. What is supposed to happen?"*). Scope
   **browsing and picking** — the asset library and the picker — and keep
   already-referenced assets resolvable. That is what "this client's folder"
   actually means, it avoids breaking published content, and it does not pretend
   to be a confidentiality boundary. Say so in the docs; the alternative
   (filtering referenced assets out of delivery) is the version that, in his
   words, *"would remove the asset"* from content that legitimately uses it.

---

## 4. Dark mode and per-app theming (#5720, #816)

**Asked:** a dark UI; and separately, brand colours and a custom logo per app.
- <https://support.squidex.io/t/dark-mode-night-mode/5720> · **Open** · 745 views · 2024-11
- <https://support.squidex.io/t/declined-custom-ui-styling/816> · Declined · 1357 views · 2019-06

**Why it matters.** 2102 views across two threads. Editors live in this UI all
day, and dark mode has moved from a preference to an expectation in tools of
this kind. The theming half is a different and commercially sharper case:
agencies putting clients into Squidex want their own logo and colours, and today
the only route is *"if you build Squidex by yourself you can customize the
colors in a single file"* — i.e. maintain a fork, which is the same trap the
Service Bus requester described in #4522.

**Why these declines are worth revisiting.** Neither was refused on technical
grounds. #5720: *"It is very likely not going to happen. Just a matter of
priority tbh."* #816: *"It is planned, but with very low priority"*, then *"I
have decided not to work on this"* — with a Trello link that is now dead. In
neither case was cost the argument. The
requester's own suggestion was to fold it into other work: *"Hopefully this will
be considered in the future when doing a UI refresh or something related."*
That is the situation now — 7.22 alone brought improved form rows, a new
responsive field menu, an option to disable the frontend UI, accessibility
fixes and an Angular 21 upgrade, and a previous
[design modernization](https://support.squidex.io/t/implemented-design-modernization/2442)
(5765 views) already shipped once.

**The honest cost, and why it buys two features.** The theme is still
build-time SCSS: `frontend/src/app/theme/_vars.scss` defines `$color-border`,
`$color-theme-brand`, `$color-dark-onboarding` and the rest as Sass variables,
and the file contains **zero CSS custom properties**. Dark mode therefore is not
a stylesheet you can add — it needs the palette converted to CSS custom
properties on `:root` so it can be swapped at runtime. That conversion is large
but mechanical and low-risk, and it is the same prerequisite for per-app
theming: once colours are runtime values, #816 becomes a settings form writing a
handful of variables, and dark mode becomes one alternate block plus a
`prefers-color-scheme` default.

**Sequence it:** convert the palette to custom properties and ship dark mode
first (one palette, no API, no persistence beyond a user preference), then
per-app branding on top. Doing theming first without the refactor is what made
it look expensive in 2019.

---

## 5. Azure Service Bus flow step (#4522)

> ✅ **Implemented.** New `AzureServiceBusFlowStep` (`Actions/AzureServiceBus/`) sends to a queue or topic, with connection string, payload, subject, session ID and message ID configurable per step.

**Asked:** contribute a Service Bus action upstream.
<https://support.squidex.io/t/inactive-azure-service-bus-action-type/4522>
· Postponed (`INACTIVE`) · 1838 views · 4 likes · **3 votes** (joint highest of
any not-implemented request, with #669 and #4305)

**Why it matters.** Service Bus is the default eventing backbone in Azure
estates, and Squidex already ships twenty rule actions / flow steps including
Kafka, Azure Queue, SignalR, Elasticsearch, OpenSearch and Typesense. The
requester was running a **forked build** to get it — *"currently we have to
merge our plugin code with Squidex's source code and then build a Docker
image"* — and offered to generalise their code and get employer approval to
contribute it. The maintainer's answer was *"Yes, why not. Sounds helpful, but
only if the credentials can be configured in the action."* Nobody refused this;
it lapsed.

**Why it is easy.** `AzureQueueFlowStep` is a template for nearly the whole
file — `[FlowStep(Title/IconImage/Display/Description/ReadMore)]` metadata,
`[Editor]`/`[Expression]`-annotated settings (which is also how the
credentials-in-the-action condition is met), a static `ClientPool` for
connection reuse, `ValidateAsync` and `ExecuteAsync`
(`backend/extensions/Squidex.Extensions/Actions/AzureQueue/AzureQueueFlowStep.cs`).
A Service Bus step swaps `CloudQueue` for `ServiceBusClient` and adds a
queue/topic name.

**Why not already covered.** Only `AzureQueue` exists. Azure Queue Storage is a
different service: no topics or subscriptions, no sessions, no FIFO guarantees,
64 KB messages. Fan-out to multiple subscribers — the usual reason to want
this — cannot be expressed with it.

**For whoever picks it up:** write a `FlowStep`, not a `RuleAction`.
`CreateContentAction` is `[Obsolete("Has been replaced by flows.")]`; existing
actions keep a thin `IConvertibleToAction` shim for migration only.

---

## 6. Give scripts a way to log (#2822)


**Asked:** `console.log` in scripting, and documentation of what `ctx` holds.
<https://support.squidex.io/t/console-log-in-scripting-ctx-values/2822>
· Unresolved · 2575 views · 2021-01

**Why it matters.** Scripting is the standard answer to requests declined
elsewhere in this very forum — *"you could also achieve that with scripting"*
(#5045), *"the only option would be scripting"* (#1229), *"create a script to
fill the field automatically"* (#330). That answer is only as good as the
debugging story, and right now a script author cannot print a value, trace a
branch, or see why nothing happened. The reporter was evaluating platforms and
concluded Squidex *"can probably do everything I need, but the lack of detailed
documentation may make execution very difficult"*. The same pain recurs as
[#5624](https://support.squidex.io/t/stale-scripts-execute-maybe-a-error/5624)
(`[STALE] Scripts execute maybe a error`) and
[#5089](https://support.squidex.io/t/getassetblurhash-asset-script-example/5089),
where a user needed someone else to write the example because they had no way to
inspect anything.

**Why it is easy.** There is a purpose-built extension point. Script functions
come from classes implementing `IJintExtension, IScriptDescriptor` —
`StringJintExtension`, `HttpJintExtension`, `DateTimeJintExtension` and four
more, registered in
`backend/src/Squidex/Config/Domain/InfrastructureServices.cs:75-93`. A
`LoggerJintExtension` in the same shape is one new file plus one registration
line, and implementing `IScriptDescriptor` puts the new function into the script
editor's autocomplete for free — the completion list is built from those
descriptors (`.../Scripting/ScriptingCompleter.cs:25`).

**Why not already covered.** There is no logging, console or debug facility in
the scripting tree at all; the only match for "debug" is `Debugger.IsAttached`
in `JintScript.cs:64`, which concerns the host process.

**Design note.** Output has to reach the *script author*, who on the cloud has
no server logs. Collecting it and returning it with the validation/preview
response is the version that helps. `JobLogMessage` shows the codebase already
has a notion of user-visible log lines to imitate.

---

## 7. Upsert mode for the "Create content" flow step (#5616)

> ✅ **Implemented.** `CreateContentFlowStep` has an optional `ID` (expression) and a `Patch` flag. With an ID it issues `UpsertContent` (create if missing, update or patch otherwise); without one it creates as before.

**Asked:** a rule that creates content when it does not exist and updates it
when it does.
<https://support.squidex.io/t/create-if-not-exists-or-update-content-if-exists-rule/5616>
· Unresolved · 786 views · 2024-08

**Why it matters.** This is the shape of every sync-from-another-system
integration — the requester lists `page`, `product`, `brand`, `marketplace`
collections kept in step with an external source. Without upsert each run either
fails on the second pass or creates duplicates, so the work moves out of rules
into bespoke code, taking the rule engine's retry, logging and configurability
with it.

**Why it is easy.** The domain already has the operation: `UpsertContent.cs`
sits beside `CreateContent.cs` in
`backend/src/Squidex.Domain.Apps.Entities/Contents/Commands/`. The flow step
simply never offers it — `CreateContentFlowStep` carries only `Data`, `Schema`,
`Client`, `Publish`
(`backend/extensions/Squidex.Extensions/Actions/CreateContent/CreateContentFlowStep.cs:38-51`).
Add an optional id/key and switch the command issued. No new infrastructure.

**The requester's framing was accurate:** *"The CreateContent rule almost does
exactly what it needs to do."*

---

## 8. Regenerate a client secret from the UI (#5807)

> ✅ **Implemented.** New `RegenerateClientSecret` command and `AppClientSecretRegenerated` event keep the client ID and issue a new secret. Exposed as `PUT apps/{app}/clients/{id}/secret` (link `secret`) and as a button next to the secret in the client settings.

**Asked:** a button to issue a new secret for an existing client.
<https://support.squidex.io/t/manually-generate-a-new-client-secret-from-within-the-ui/5807>
· Open · 1383 views · 2025-01

**Why it matters.** Rotation is routine security hygiene — after a leak, after
someone leaves, on a schedule. The only route today is deleting the client and
creating a new one, which changes the **client id** too and breaks every
integration using it. A 30-second rotation becomes a coordinated redeployment,
so in practice secrets never get rotated.

**Why it is easy.** The generation logic exists: `AttachClient` sets
`Secret = RandomHash.New()`
(`backend/src/Squidex.Domain.Apps.Entities/Apps/Commands/AttachClient.cs:22`).
The work is letting an existing client take a new one — `UpdateClient` (same
folder) carries only `Name`, `Role`, `ApiCallsLimit`, `ApiTrafficLimit`,
`AllowAnonymous` — plus an event and a button.

**Why not already covered.** `AppClientsController` exposes only `HttpPost`,
`HttpPut` and `HttpDelete` (lines 63/89/113). Nothing can change a secret.

**The maintainer already agreed:** *"Actually not, but should be easy to build."*
The thread then stalled on *"The secret never expires"* — which is the reason
rotation must be manual, not a reason to omit it.

---

## 9. Show which fields are required when publishing (#4375)

> ✅ **Implemented.** Fields with *Required when publishing* (and not plain *Required*) show an orange `*` with a tooltip next to the label in the content editor (`field-editor.component.html`).

**Asked:** a visual indicator for fields validated by *Required when publishing*,
like the asterisk plain *Required* fields get.
<https://support.squidex.io/t/very-unlikely-highlight-fields-that-are-required-when-publishing/4375>
· Declined (`VERY_UNLIKELY`) · 1707 views · 2 votes

**Why it matters.** Precise complaint: nothing marks the field, so publishing
fails and the editor then hunts for the cause. It hits the least technical users
at the moment they are trying to ship.

**Why it is easy.** One template binding. The marker already exists for the
other flag:

```
frontend/src/app/features/content/shared/forms/field-editor.component.html:7
{{ field.displayName }} {{ displaySuffix }}
<span class="field-required" [class.hidden]="!field.properties.isRequired">*</span>
```

`isRequiredOnPublish` is on the same properties object — edited in
`field-form-validation.component.html:6`, present in
`shared/model/generated.ts:3124`. A second marker bound to it, a CSS rule and an
i18n key.

**Why not already covered.** Nothing in the content editor reads
`isRequiredOnPublish`; the only occurrences are the schema editor and the
generated model.

**On the decline.** The reply was *"Just write a hint for your users 😉"* and the
topic auto-closed after two days — asking every customer to document around a
missing marker whose data the UI already holds. Cheapest item here and worth
reversing.

---

## 10. Let users upload a duplicate asset anyway (#3144)

> ✅ **Implemented.** Uploading a new file that the backend reports as a duplicate now asks the user whether to upload it anyway; confirming retries the upload with `duplicate=true` (`asset-uploader.state.ts`), declining keeps the existing asset as before.

**Asked:** stop blocking a file because an identical one exists in another
folder.
<https://support.squidex.io/t/declined-assets-flagged-as-duplicate-from-another-folder/3144>
· Declined · 1356 views

**Why it matters.** The uploader hits a wall with no way past it, and folders
are routinely used to separate clients, campaigns or locales where holding the
same file twice is deliberate.

**Why it is easy.** The backend already supports the override. `CreateAsset` and
`UpsertAsset` both carry a `Duplicate` flag, and
`AssetCommandMiddleware.UploadWithDuplicateCheckAsync` skips the check when it
is set, otherwise completing with an `AssetDuplicate` result
(`.../Assets/DomainObject/AssetCommandMiddleware.cs:57-84`). Only the UI is
missing: handle `AssetDuplicate`, confirm, retry with `duplicate: true`.

**Why not already covered.** Nothing in the frontend references duplicates — the
flag is reachable only by calling the API directly.

**The maintainer already scoped it:** *"it is only a flag in the API, so I can
probably enhance the UI to upload the file anyway. Like with an extra confirm
dialog or so."*

**Worth knowing:** the check is `FindByHashAsync(context, fileHash, fileName,
fileSize)` — content hash *and* name *and* size, app-wide — so it only fires for
genuinely identical files. That makes "upload anyway" the right fix and a
per-folder scope change unnecessary.

---

## Rejected because already covered

Verified in the source, not taken from the thread. Two had been postponed or
declined and have since been implemented without the thread being updated —
which is the main reason this check is worth running:

| Request | Views | Why it was dropped |
| --- | --- | --- |
| [#3321 Fetch concrete references with GraphQL](https://support.squidex.io/t/unlikely-fetch-concrete-references-with-graphql-and-the-other-array-fields/3321) | 2170 | **Implemented since being postponed.** `ContentUnionGraphType` / `ComponentUnionGraphType` exist and are used for reference fields, components and rich text (`FieldVisitor.cs:285,309`, `RichTextGraphType.cs:55`). |
| [#1852 Update the markdown editor](https://support.squidex.io/t/declined-update-markdown-editor-component/1852) | 2130 | **Superseded since being declined.** A new structured content editor shipped (`CHANGELOG.md:300,323`), with the old one kept as a sample editor. |
| [#3442 SignalR notification rule](https://support.squidex.io/t/add-signalr-notification-rule/3442) | 1991 | Already shipped — `backend/extensions/Squidex.Extensions/Actions/SignalR/` has `SignalRAction`, `SignalRFlowStep`, `SignalRPlugin`. |
| [#5672 Hide "business account" / signup on login](https://support.squidex.io/t/login-popup-possiblity-to-remove-enter-business-account-and-signup/5672) | 940 | Already configurable — `Login.cshtml:62` gates that form on `HasCustomAuth`, set from `allowCustomDomains` (`AccountController.cs:217`). |
| [#4499 Change image format via URL](https://support.squidex.io/t/assets-change-image-format-via-url/4499) | 1453 | Already supported — `Format` and `Auto` on `AssetContentQueryDto` (lines 88, 106). |
| [#538 Preview URL supports one placeholder](https://support.squidex.io/t/solved-preview-url-support-only-1-placeholder-increase-the-limit/538) | 969 | Fixed — `interpolate()` replaces every match (`frontend/src/app/framework/utils/interpolator.ts:13`). |
| [#1168 Expose schemas as JSON Schema](https://support.squidex.io/t/declined-squidexs-json-schema/1168) | 3500 | Covered by the generated OpenAPI documents, which embed JSON Schema. |
| [#1625 Content revisions / versions](https://support.squidex.io/t/content-revisions-versions/1625) | 4745 | A maintainer-authored 2020 concept for reworking the workflow system; the workflow feature it describes shipped. |

## Re-examined declines that still hold

The blocker is unchanged, so these stay off the list:

- [#3025 Filter assets not linked to any content](https://support.squidex.io/t/declined-filter-by-assets-not-linked-to-any-contents/3025) (1380 views) — *"a lot of effort to maintain a secondary index"*. Still true; nothing in the archive or the code changes it.
- [#2306 Backup all apps at once](https://support.squidex.io/t/declined-backup-all-apps-with-one-click/2306) (1032) — *"you can easier backup your mongodb database"*. The job framework would make it easy now, but the objection was value, not cost, and it is a fair one.
- [#3568 Full-text search improvements](https://support.squidex.io/t/unlikely-content-fulltext-search-improvement/3568) (2246) — answered with a detailed technical rebuttal, and search has since moved on independently (Atlas Search option at `appsettings.json:736`, plus searchable string fields in 7.22).
- [#4462 Localizable components](https://support.squidex.io/t/localizable-components/4462) (2179) — declined on a defensible design ground: localized fields containing localized subfields.
- [#1675 Taxonomies](https://support.squidex.io/t/taxonomies-for-content-items/1675) (4983) and [#1667 Tree view](https://support.squidex.io/t/unlikely-tree-view-for-content/1667) (3737) — the most-demanded cluster in the archive, but the maintainer's objection is a design one he has not resolved (*"we would create a second system for references"*), and it is unresolved by anything in the current code. Worth a decision, not a ticket.

## Dropped from the previous revision

Removed to make room for larger items, not because they stopped being worth
doing. Each is still small and still uncovered:

- [#4363 Say when auto-translate is not configured](https://support.squidex.io/t/auto-translation-with-google-translate-api-and-frontend-settings/4363) — **7038 views**, the most-viewed unresolved request in the archive. The translate action renders unconditionally (`content-field.component.html:119-125`) with no check for a configured provider, and the key lives only in `appsettings.json:819`. Cheap to fix and high-traffic; promote it if you want an eleventh.
- [#1363 Respect `maxItems` in the asset editor](https://support.squidex.io/t/postponed-single-asset-media-image-type/1363) (2622) — `array-editor.component.ts:91,104` gates adding at the limit; `assets-editor.component.ts` does not.
- [#2183 Larger page sizes in list views](https://support.squidex.io/t/declined-content-view-display-250/2183) (1189) — one constant, `pager.component.ts:15`. The decline's cloud-performance concern argues for making it configurable, not for refusing.
- [#1042 Hide UI sections a role should not see](https://support.squidex.io/t/ui-customization/1042) (1499) — a separate ask from theming, despite the title: hiding the schemas section from content editors and specific dashboard charts. *"Why should the user who only has very minimum content access be able to see the whole site schema?"* The maintainer's reply was *"we could make it more customizable and hide specific charts"* and *"I think it has already been requested somewhere, but it is not on my roadmap yet"*. 7.22 added an option to disable the whole frontend, which is all-or-nothing rather than per-section. A reasonable follow-on to item 3, since both are permission-shaped.
- Self-service password reset (#5997) — **dropped at your direction**; most deployments use an external identity provider, so it would mostly be unused code. No trace of it exists in the backend if it is ever wanted.

## Caveats

- Status reflects the forum's categorisation at archive time (2026-09-20). Where
  it mattered, the claim was checked in the source and the file and line cited.
- Size labels come from reading the code, not building it, and assume the change
  stays within the extension point named. Items 1–4 have genuine unknowns; items
  3 and 4 each need a design decision before work starts, and both decisions are
  spelled out above.
- `D:\Squidex` is on branch `stability`, ahead of public `master`. Anything
  merged after `fc026a3` is not reflected here.
