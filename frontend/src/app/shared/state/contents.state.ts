/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { EMPTY, Observable, of } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import { DateTime, debug, DialogService, getPagingInfo, ListState, shareSubscribed, State, Types, Version, Versioned } from '@app/framework';
import { BulkResultDto, BulkUpdateContentsDto, BulkUpdateContentsJobDto, ContentDto, ContentsDto, IBulkUpdateContentsJobDto, ServerErrorDto } from '../model';
import { ContentsService, StatusInfo } from '../services/contents.service';
import { Query } from '../services/query';
import { AppsState } from './apps.state';
import { SavedQuery } from './queries';
import { SchemasState } from './schemas.state';

/* eslint-disable @typescript-eslint/no-throw-literal */

interface Snapshot extends ListState<Query> {
    // The current contents.
    contents: ReadonlyArray<ContentDto>;

    // The field names to select.
    fieldNames: ReadonlyArray<string> | null;

    // The referencing content id.
    referencing?: string;

    // The reference content id.
    references?: string;

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
        this.projectFrom(this.statuses, getStatusQueries);

    public get appId() {
        return this.appsState.appId;
    }

    public get appName() {
        return this.appsState.appName;
    }

    protected constructor(name: string,
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService,
    ) {
        super({
            contents: [],
            fieldNames: null,
            page: 0,
            pageSize: 10,
            total: 0,
            validationResults: {},
        });

        debug(this, name);
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
            this.contentsService.getContent(this.appName, this.schemaName, id).pipe(catchError(() => of(null)));
    }

    public loadReferences(contentId: string, update: Partial<Snapshot> = {}) {
        this.resetState({ references: contentId, referencing: undefined, ...update });

        return this.loadInternal(false, true);
    }

    public loadReferencing(contentId: string, update: Partial<Snapshot> = {}) {
        this.resetState({ referencing: contentId, references: undefined, ...update });

        return this.loadInternal(false, true);
    }

    public load(isReload = false, noSlowTotal = true, update: Partial<Snapshot> = {}): Observable<any> {
        if (!isReload) {
            this.resetState({ selectedContent: this.snapshot.selectedContent, fieldNames: this.snapshot.fieldNames, ...update }, 'Loading Intial');
        }

        return this.loadInternal(isReload, noSlowTotal);
    }

    public loadIfNotLoaded(): Observable<any> {
        if (this.snapshot.isLoaded) {
            return EMPTY;
        }

        return this.loadInternal(false, true);
    }

    public setFieldNames(fieldNames: ReadonlyArray<string> | null) {
        this.next( { fieldNames }, 'Set field names.');
    }

    private loadInternal(isReload: boolean, noSlowTotal: boolean) {
        return this.loadInternalCore(isReload, noSlowTotal).pipe(shareSubscribed(this.dialogs));
    }

    private loadInternalCore(isReload: boolean, noSlowTotal: boolean) {
        if (!this.appName) {
            return EMPTY;
        }

        this.next({ isLoading: true }, 'Loading Started');

        const { references, referencing } = this.snapshot;
        const query = createQuery(this.snapshot, noSlowTotal);

        let content$: Observable<ContentsDto>;

        if (referencing && this.schemaName) {
            content$ = this.contentsService.getContentReferencing(this.appName, this.schemaName, referencing, query);
        } else if (referencing) {
            content$ = this.contentsService.getAllContents(this.appName, { referencing }, query);
        } else if (references && this.schemaName) {
            content$ = this.contentsService.getContentReferences(this.appName, this.schemaName, references, query);
        } else if (references) {
            content$ = this.contentsService.getAllContents(this.appName, { references }, query);
        } else if (this.schemaName) {
            content$ = this.contentsService.getContents(this.appName, this.schemaName, query);
        } else {
            return EMPTY;
        }

        return content$.pipe(
            tap(({ total, items: contents, canCreate, canCreateAndPublish, statuses }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:contents.reloaded');
                }

                return this.next(s => {
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
                        statuses: s.statuses || statuses,
                        total: total >= 0 ? total : s.total,
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
        const job: Partial<BulkUpdateContentsJobDto> = { type: 'Validate' };

        return this.bulkMany(contents, false, job).pipe(
            tap(results => {
                return this.next(s => {
                    const validationResults = { ...s.validationResults || {} };

                    for (const result of results) {
                        validationResults[result.contentId!] = !result.error;
                    }

                    return { ...s, validationResults };
                }, 'Validated');
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public changeManyStatus(contents: ReadonlyArray<ContentDto>, status: string, dueTime?: DateTime | undefined): Observable<any> {
        const job: Partial<IBulkUpdateContentsJobDto> = { type: 'ChangeStatus', status, dueTime };

        return this.bulkWithRetry(contents, job,
                'i18n:contents.unpublishReferrerConfirmTitle',
                'i18n:contents.unpublishReferrerConfirmText',
                'unpublishReferencngContent').pipe(
            switchMap(() => this.reloadContents(contents)), shareSubscribed(this.dialogs));
    }

    public deleteMany(contents: ReadonlyArray<ContentDto>) {
        const job: Partial<IBulkUpdateContentsJobDto> = { type: 'Delete' };

        return this.bulkWithRetry(contents, job,
                'i18n:contents.deleteReferrerConfirmTitle',
                'i18n:contents.deleteReferrerConfirmText',
                'deleteReferencngContent').pipe(
            switchMap(() => this.loadInternalCore(false, true)), shareSubscribed(this.dialogs));
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
        if (!this.next({ query, page: 0, total: 0 }, 'Loading Searched')) {
            return EMPTY;
        }

        return this.loadInternal(false, true);
    }

    public page(paging: { page: number; pageSize: number }) {
        if (!this.next(paging, 'Loading Done')) {
            return EMPTY;
        }

        return this.loadInternal(false, true);
    }

    private reloadContents(contents: ReadonlyArray<ContentDto>) {
        this.next({ isLoading: true }, 'Loading Done');

        return this.contentsService.getAllContents(this.appName, { ids: contents.map(x => x.id) }).pipe(
            tap(updates => {
                return this.next(s => {
                    let contents = s.contents, selectedContent = s.selectedContent;

                    for (const content of updates.items) {
                        contents = contents.replacedBy('id', content);

                        selectedContent =
                            s.selectedContent?.id !== content.id ?
                            s.selectedContent :
                            content;
                    }

                    return { ...s, contents, selectedContent };
                });
            }),
            finalize(() => {
                this.next({ isLoading: false }, 'Loading Done');
            }));
    }

    private replaceContent(content: ContentDto, oldVersion?: number, updateText?: string) {
        if (!oldVersion || oldVersion != content.version) {
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

    private bulkWithRetry(contents: ReadonlyArray<ContentDto>, job: Partial<IBulkUpdateContentsJobDto>, confirmTitle: string, confirmText: string, confirmKey: string): Observable<ReadonlyArray<BulkResultDto>> {
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
                        throw error;
                    } else {
                        this.dialogs.notifyError(error.toError());
                    }
                }
            }));
    }

    private bulkMany(contents: ReadonlyArray<ContentDto>, checkReferrers: boolean, job: Partial<IBulkUpdateContentsJobDto>): Observable<ReadonlyArray<BulkResultDto>> {
        const update = new BulkUpdateContentsDto({
            // This is set to true by default, so we turn it off here.
            optimizeValidation: false,
            doNotValidate: false,
            doNotScript: false,
            jobs: contents.map(x => new BulkUpdateContentsJobDto(({
                id: x.id,
                schema: x.schemaName,
                status: undefined,
                expectedVersion: x.version,
                ...job,
            }))),
            checkReferrers,
        });

        return this.contentsService.bulkUpdate(this.appName, this.schemaName, update as any);
    }

    public abstract get schemaName(): string;
}

function isReferrerError(error?: ServerErrorDto) {
    return error?.errorCode === 'OBJECT_REFERENCED';
}

@Injectable({
    providedIn: 'root',
})
export class ContentsState extends ContentsStateBase {
    constructor(appsState: AppsState, contentsService: ContentsService, dialogs: DialogService,
        private readonly schemasState: SchemasState,
    ) {
        super('contents', appsState, contentsService, dialogs);
    }

    public get schemaName() {
        return this.schemasState.schemaName;
    }
}

@Injectable()
export class ComponentContentsState extends ContentsStateBase {
    public schema!: { name: string };

    constructor(
        appsState: AppsState, contentsService: ContentsService, dialogs: DialogService,
    ) {
        super('componentContents', appsState, contentsService, dialogs);
    }

    public get schemaName() {
        return this.schema.name;
    }
}

function getStatusQueries(statuses: ReadonlyArray<StatusInfo> | undefined): ReadonlyArray<SavedQuery> {
    return statuses?.map(buildStatusQuery) || [];
}

function createQuery(snapshot: Snapshot, noSlowTotal: boolean) {
    const {
        fieldNames,
        page,
        pageSize,
        query,
        total,
    } = snapshot;

    const result: any = { take: pageSize, skip: pageSize * page, noSlowTotal, fieldNames };

    if (query) {
        result.query = query;
    }

    if (page > 0 && total > 0) {
        result.noTotal = true;
    }

    return result;
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
