/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, ErrorDto, getPagingInfo, ListState, shareSubscribed, State, Types, Version, Versioned } from '@app/framework';
import { EMPTY, Observable, of } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import { BulkResultDto, BulkUpdateJobDto, ContentDto, ContentsDto, ContentsService } from './../services/contents.service';
import { AppsState } from './apps.state';
import { SavedQuery } from './queries';
import { Query, StatusInfo } from './query';
import { SchemasState } from './schemas.state';

interface Snapshot extends ListState<Query> {
    // The current contents.
    contents: ReadonlyArray<ContentDto>;

    // The referencing content id.
    referencing?: string;

    // The reference content id.
    reference?: string;

    // The statuses.
    statuses?: ReadonlyArray<StatusInfo>;

    // The selected content.
    selectedContent?: ContentDto | null;

    // The validation results.
    validationResults: { [id: string]: boolean };

    // Indicates if the user can create a content.
    canCreate?: boolean;

    // Indicates if the user can create and publish a content.
    canCreateAndPublish?: boolean;
}

export abstract class ContentsStateBase extends State<Snapshot> {
    public selectedContent: Observable<ContentDto | undefined | null> =
        this.project(x => x.selectedContent, Types.equals);

    public contents =
        this.project(x => x.contents);

    public paging =
        this.project(x => getPagingInfo(x, x.contents.length));

    public query =
        this.project(x => x.query);

    public validationResults =
        this.project(x => x.validationResults);

    public isLoaded =
        this.project(x => x.isLoaded === true);

    public isLoading =
        this.project(x => x.isLoading === true);

    public canCreate =
        this.project(x => x.canCreate === true);

    public canCreateAndPublish =
        this.project(x => x.canCreateAndPublish === true);

    public canCreateAny =
        this.project(x => x.canCreate === true || x.canCreateAndPublish === true);

    public statuses =
        this.project(x => x.statuses);

    public statusQueries =
        this.projectFrom(this.statuses, buildStatusQueries);

    public get appName() {
        return this.appsState.appName;
    }

    public get appId() {
        return this.appsState.appId;
    }

    protected constructor(name: string,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService,
    ) {
        super({
            contents: [],
            page: 0,
            pageSize: 10,
            total: 0,
            validationResults: {},
        }, name);
    }

    public select(id: string | null): Observable<ContentDto | null> {
        return this.loadContent(id).pipe(
            tap(content => {
                this.next(s => {
                    const contents = content ? s.contents.replacedBy('id', content) : s.contents;

                    return { ...s, selectedContent: content, contents };
                }, 'Selected');
            }));
    }

    private loadContent(id: string | null) {
        return !id ?
            of(null) :
            of(this.snapshot.contents.find(x => x.id === id)).pipe(
                switchMap(content => {
                    if (!content) {
                        return this.contentsService.getContent(this.appName, this.schemaName, id).pipe(catchError(() => of(null)));
                    } else {
                        return of(content);
                    }
                }));
    }

    public loadReference(contentId: string, update: Partial<Snapshot> = {}) {
        this.resetState({ reference: contentId, referencing: undefined, ...update });

        return this.loadInternal(false);
    }

    public loadReferencing(contentId: string, update: Partial<Snapshot> = {}) {
        this.resetState({ referencing: contentId, reference: undefined, ...update });

        return this.loadInternal(false);
    }

    public load(isReload = false, update: Partial<Snapshot> = {}): Observable<any> {
        if (!isReload) {
            this.resetState({ selectedContent: this.snapshot.selectedContent, ...update }, 'Loading Intial');
        }

        return this.loadInternal(isReload);
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    private loadInternal(isReload: boolean) {
        return this.loadInternalCore(isReload).pipe(shareSubscribed(this.dialogs));
    }

    private loadInternalCore(isReload: boolean) {
        if (!this.appName || !this.schemaName) {
            return EMPTY;
        }

        this.next({ isLoading: true }, 'Loading Done');

        const { page, pageSize, query, reference, referencing } = this.snapshot;

        const q: any = { take: pageSize, skip: pageSize * page };

        if (query) {
            q.query = query;
        }

        let content$: Observable<ContentsDto>;

        if (referencing) {
            content$ = this.contentsService.getContentReferencing(this.appName, this.schemaName, referencing, q);
        } else if (reference) {
            content$ = this.contentsService.getContentReferences(this.appName, this.schemaName, reference, q);
        } else {
            content$ = this.contentsService.getContents(this.appName, this.schemaName, q);
        }

        return content$.pipe(
            tap(({ total, items: contents, canCreate, canCreateAndPublish, statuses }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:contents.reloaded');
                }

                return this.next(s => {
                    statuses = s.statuses || statuses;

                    let selectedContent = s.selectedContent;

                    if (selectedContent) {
                        selectedContent = contents.find(x => x.id === selectedContent!.id) || selectedContent;
                    }

                    return {
                        ...s,
                        canCreate,
                        canCreateAndPublish,
                        isLoaded: true,
                        isLoading: false,
                        contents,
                        selectedContent,
                        statuses,
                        total,
                    };
                }, 'Loading Success');
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }));
    }

    public loadVersion(content: ContentDto, version: Version): Observable<Versioned<any>> {
        return this.contentsService.getVersionData(this.appName, this.schemaName, content.id, version).pipe(
            shareSubscribed(this.dialogs));
    }

    public create(data: any, publish: boolean, id = ''): Observable<ContentDto> {
        return this.contentsService.postContent(this.appName, this.schemaName, data, publish, id).pipe(
            tap(payload => {
                this.dialogs.notifyInfo('i18n:contents.created');

                return this.next(s => {
                    const contents = [payload, ...s.contents].slice(s.pageSize);

                    return { ...s, contents, total: s.total + 1 };
                }, 'Created');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public validate(contents: ReadonlyArray<ContentDto>): Observable<any> {
        const job: Partial<BulkUpdateJobDto> = { type: 'Validate' };

        return this.bulkMany(contents, false, job).pipe(
            tap(results => {
                return this.next(s => {
                    const validationResults = { ...s.validationResults || {} };

                    for (const result of results) {
                        validationResults[result.contentId] = !result.error;
                    }

                    return { ...s, validationResults };
                }, 'Validated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public changeManyStatus(contents: ReadonlyArray<ContentDto>, status: string, dueTime?: string | null): Observable<any> {
        const job: Partial<BulkUpdateJobDto> = { type: 'ChangeStatus', status, dueTime };

        return this.bulkWithRetry(contents, job,
                'i18n:contents.unpublishReferrerConfirmTitle',
                'i18n:contents.unpublishReferrerConfirmText',
                'unpublishReferencngContent').pipe(
            switchMap(() => this.loadInternalCore(false)), shareSubscribed(this.dialogs));
    }

    public deleteMany(contents: ReadonlyArray<ContentDto>) {
        const job: Partial<BulkUpdateJobDto> = { type: 'Delete' };

        return this.bulkWithRetry(contents, job,
                'i18n:contents.deleteReferrerConfirmTitle',
                'i18n:contents.deleteReferrerConfirmText',
                'deleteReferencngContent').pipe(
            switchMap(() => this.loadInternalCore(false)), shareSubscribed(this.dialogs));
}

    public update(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public cancelStatus(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.cancelStatus(this.appName, content, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public createDraft(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.createVersion(this.appName, content, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public deleteDraft(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.deleteVersion(this.appName, content, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs));
    }

    public patch(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.patchContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.replaceContent(updated, content.version, 'i18n:contents.updated');
            }),
            shareSubscribed(this.dialogs));
    }

    public search(query?: Query): Observable<any> {
        if (!this.next({ query, page: 0 }, 'Loading Searched')) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    public page(paging: { page: number; pageSize: number }) {
        if (!this.next(paging, 'Loading Done')) {
            return EMPTY;
        }

        return this.loadInternal(false);
    }

    private replaceContent(content: ContentDto, oldVersion?: Version, updateText?: string) {
        if (!oldVersion || !oldVersion.eq(content.version)) {
            if (updateText) {
                this.dialogs.notifyInfo(updateText);
            }

            return this.next(s => {
                const contents = s.contents.replacedBy('id', content);

                const selectedContent =
                    s.selectedContent?.id !== content.id ?
                    s.selectedContent :
                    content;

                return { ...s, contents, selectedContent };
            }, 'Updated');
        }

        return false;
    }

    private bulkWithRetry(contents: ReadonlyArray<ContentDto>, job: Partial<BulkUpdateJobDto>, confirmTitle: string, confirmText: string, confirmKey: string): Observable<ReadonlyArray<BulkResultDto>> {
        return this.bulkMany(contents, true, job).pipe(
            switchMap(results => {
                const referrerFailures = results.filter(x => isReferrerError(x.error));

                if (referrerFailures.length > 0) {
                    const failed = contents.filter(x => referrerFailures.find(r => r.contentId === x.id));

                    return this.dialogs.confirm(confirmTitle, confirmText, confirmKey).pipe(
                        switchMap(confirmed => {
                            if (confirmed) {
                                return this.bulkMany(failed, false, job);
                            } else {
                                return of([]);
                            }
                        }),
                        map(retried => {
                            const nonRetried = results.filter(x => !retried.find(y => y.contentId === x.contentId));

                            return [...nonRetried, ...retried];
                        }),
                    );
                } else {
                    return of(results);
                }
            }),
            tap(results => {
                const errors = results.filter(x => !!x.error);

                if (errors.length > 0) {
                    const error = errors[0].error!;

                    if (errors.length >= contents.length) {
                        // eslint-disable-next-line @typescript-eslint/no-throw-literal
                        throw error;
                    } else {
                        this.dialogs.notifyError(error);
                    }
                }
            }));
    }

    private bulkMany(contents: ReadonlyArray<ContentDto>, checkReferrers: boolean, job: Partial<BulkUpdateJobDto>): Observable<ReadonlyArray<BulkResultDto>> {
        const update = {
            doNotScript: false,
            jobs: contents.map(x => ({
                id: x.id,
                schema: x.schemaName,
                status: undefined,
                expectedVersion: parseInt(x.version.value, 10),
                ...job,
            })),
            checkReferrers,
        };

        return this.contentsService.bulkUpdate(this.appName, this.schemaName, update as any);
    }

    public abstract get schemaName(): string;
}

function isReferrerError(error?: ErrorDto) {
    return error?.errorCode === 'OBJECT_REFERENCED';
}

@Injectable()
export class ContentsState extends ContentsStateBase {
    constructor(appsState: AppsState, contentsService: ContentsService, dialogs: DialogService,
        private readonly schemasState: SchemasState,
    ) {
        super('Contents', appsState, contentsService, dialogs);
    }

    public get schemaName() {
        return this.schemasState.schemaName;
    }
}

@Injectable()
export class ComponentContentsState extends ContentsStateBase {
    public schema: { name: string };

    constructor(
        appsState: AppsState, contentsService: ContentsService, dialogs: DialogService,
    ) {
        super('Components Contents', appsState, contentsService, dialogs);
    }

    public get schemaName() {
        return this.schema.name;
    }
}

function buildStatusQueries(statuses: ReadonlyArray<StatusInfo> | undefined): ReadonlyArray<SavedQuery> {
    return statuses?.map(buildStatusQuery) || [];
}

function buildStatusQuery(s: StatusInfo) {
    const query = {
        filter: {
            and: [
                { path: 'status', op: 'eq', value: s.status },
            ],
        },
    };

    return ({ name: s.status, color: s.color, query });
}
