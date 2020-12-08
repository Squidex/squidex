/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { DialogService, Pager, shareSubscribed, State, StateSynchronizer, Types, Version, Versioned } from '@app/framework';
import { EMPTY, Observable, of } from 'rxjs';
import { catchError, finalize, map, switchMap, tap } from 'rxjs/operators';
import { BulkResultDto, BulkUpdateJobDto, ContentDto, ContentsDto, ContentsService, StatusInfo } from './../services/contents.service';
import { AppsState } from './apps.state';
import { SavedQuery } from './queries';
import { Query, QuerySynchronizer } from './query';
import { SchemasState } from './schemas.state';

interface Snapshot {
    // The current comments.
    contents: ReadonlyArray<ContentDto>;

    // The pagination information.
    contentsPager: Pager;

    // The query to filter and sort contents.
    contentsQuery?: Query;

    // Indicates if the contents are loaded.
    isLoaded?: boolean;

    // Indicates if the contents are loading.
    isLoading?: boolean;

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
    public selectedContent: Observable<ContentDto | null | undefined> =
        this.project(x => x.selectedContent, Types.equals);

    public contents =
        this.project(x => x.contents);

    public contentsPager =
        this.project(x => x.contentsPager);

    public contentsQuery =
        this.project(x => x.contentsQuery);

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
        this.projectFrom(this.statuses, x => buildStatusQueries(x));

    public get appName() {
        return this.appsState.appName;
    }

    public get appId() {
        return this.appsState.appId;
    }

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
        super({
            contents: [],
            contentsPager: new Pager(0),
            validationResults: {}
        });
    }

    public select(id: string | null): Observable<ContentDto | null> {
        return this.loadContent(id).pipe(
            tap(content => {
                this.next(s => {
                    const contents = content ? s.contents.replaceBy('id', content) : s.contents;

                    return { ...s, selectedContent: content, contents };
                });
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

    public loadAndListen(synchronizer: StateSynchronizer) {
        synchronizer.mapTo(this)
            .keep('selectedContent')
            .withPager('contentsPager', 'contents', 10)
            .withSynchronizer('contentsQuery', QuerySynchronizer.INSTANCE)
            .whenSynced(() => this.loadInternal(false))
            .build();
    }

    public loadReference(contentId: string) {
        this.resetState({ reference: contentId });

        return this.loadInternal(false);
    }

    public loadReferencing(contentId: string) {
        this.resetState({ referencing: contentId });

        return this.loadInternal(false);
    }

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState({ selectedContent: this.snapshot.selectedContent });
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

        this.next({ isLoading: true });

        const {
            contentsPager: { take, skip },
            contentsQuery,
            reference,
            referencing
        } = this.snapshot;

        const query: any = { take, skip };

        if (contentsQuery) {
            query.query = this.snapshot.contentsQuery;
        }

        let content$: Observable<ContentsDto>;

        if (referencing) {
            content$ = this.contentsService.getContentReferencing(this.appName, this.schemaName, referencing, query);
        } else if (reference) {
            content$ = this.contentsService.getContentReferences(this.appName, this.schemaName, reference, query);
        } else {
            content$ = this.contentsService.getContents(this.appName, this.schemaName, query);
        }

        return content$.pipe(
            tap(({ total, items: contents, canCreate, canCreateAndPublish, statuses }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('i18n:contents.reloaded');
                }

                return this.next(s => {
                    const contentsPager = s.contentsPager.setCount(total);

                    statuses = s.statuses || statuses;

                    let selectedContent = s.selectedContent;

                    if (selectedContent) {
                        selectedContent = contents.find(x => x.id === selectedContent!.id) || selectedContent;
                    }

                    return {
                        ...s,
                        canCreate,
                        canCreateAndPublish,
                        contents,
                        contentsPager,
                        isLoaded: true,
                        isLoading: false,
                        selectedContent,
                        statuses
                    };
                });
            }),
            finalize(() => {
                this.next({ isLoading: false });
            }));
    }

    public loadVersion(content: ContentDto, version: Version): Observable<Versioned<any>> {
        return this.contentsService.getVersionData(this.appName, this.schemaName, content.id, version).pipe(
            shareSubscribed(this.dialogs));
    }

    public create(request: any, publish: boolean): Observable<ContentDto> {
        return this.contentsService.postContent(this.appName, this.schemaName, request, publish).pipe(
            tap(payload => {
                this.dialogs.notifyInfo('i18n:contents.created');

                return this.next(s => {
                    const contentsPager = s.contentsPager.incrementCount();
                    const contents = [payload, ...s.contents].slice(contentsPager.page);

                    return { ...s, contents, contentsPager };
                });
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
                });
            }),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public changeManyStatus(contents: ReadonlyArray<ContentDto>, status: string, dueTime?: string | null): Observable<any> {
        const job: Partial<BulkUpdateJobDto> = { type: 'ChangeStatus', status, dueTime };

        return this.bulkWithRetry(contents, job,
            'i18n:contents.unpublishReferrerConfirmTitle',
            'i18n:contents.unpublishReferrerConfirmText',
            'unpublishReferencngContent').pipe(
                switchMap(() => this.loadInternalCore(false)),
                shareSubscribed(this.dialogs));
    }

    public deleteMany(contents: ReadonlyArray<ContentDto>) {
        const job: Partial<BulkUpdateJobDto> = { type: 'Delete' };

        return this.bulkWithRetry(contents, job,
            'i18n:contents.deleteReferrerConfirmTitle',
            'i18n:contents.deleteReferrerConfirmText',
            'deleteReferencngContent').pipe(
                switchMap(() => this.loadInternalCore(false)),
                shareSubscribed(this.dialogs));
    }

    public update(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, content, request, content.version).pipe(
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

    public search(contentsQuery?: Query): Observable<any> {
        this.next(s => ({
            ...s,
            contentsPager: s.contentsPager.reset(),
            contentsQuery
        }));

        return this.loadInternal(false);
    }

    public setPager(contentsPager: Pager) {
        this.next({ contentsPager });

        return this.loadInternal(false);
    }

    private replaceContent(content: ContentDto, oldVersion?: Version, updateText?: string) {
        if (!oldVersion || !oldVersion.eq(content.version)) {
            if (updateText) {
                this.dialogs.notifyInfo(updateText);
            }

            return this.next(s => {
                const contents = s.contents.replaceBy('id', content);

                const selectedContent =
                    s.selectedContent &&
                        s.selectedContent.id === content.id ?
                        content :
                        s.selectedContent;

                return { ...s, contents, selectedContent };
            });
        }
    }

    private bulkWithRetry(contents: ReadonlyArray<ContentDto>, job: Partial<BulkUpdateJobDto>,
        confirmTitle: string,
        confirmText: string,
        confirmKey: string): Observable<ReadonlyArray<BulkResultDto>> {
        return this.bulkMany(contents, true, job).pipe(
            switchMap(results => {
                const failed = contents.filter(x => results.find(r => r.contentId === x.id)?.error?.statusCode === 400);

                if (failed.length > 0) {
                    return this.dialogs.confirm(confirmTitle, confirmText, confirmKey).pipe(
                        switchMap(confirmed => {
                            if (confirmed) {
                                return this.bulkMany(failed, false, job);
                            } else {
                                return of([]);
                            }
                        }),
                        map(results2 => {
                            return [...results, ...results2];
                        })
                    );
                } else {
                    return of(results);
                }
            }),
            tap(results => {
                const errors = results.filter(x => !!x.error);

                if (errors.length > 0) {
                    const errror = errors[0].error!;

                    if (errors.length >= contents.length) {
                        throw errror;
                    } else {
                        this.dialogs.notifyError(errror);
                    }
                }
            }));
    }

    private bulkMany(contents: ReadonlyArray<ContentDto>, checkReferrers: boolean, job: Partial<BulkUpdateJobDto>): Observable<ReadonlyArray<BulkResultDto>> {
        const update = {
            jobs: contents.map(x => ({
                id: x.id,
                schema: x.schemaName,
                status: undefined,
                expectedVersion: parseInt(x.version.value, 10),
                ...job
            })),
            checkReferrers
        };

        return this.contentsService.bulkUpdate(this.appName, this.schemaName, update as any);
    }

    public abstract get schemaName(): string;
}

@Injectable()
export class ContentsState extends ContentsStateBase {
    constructor(appsState: AppsState, contentsService: ContentsService, dialogs: DialogService,
        private readonly schemasState: SchemasState
    ) {
        super(appsState, contentsService, dialogs);
    }

    public get schemaName() {
        return this.schemasState.schemaName;
    }
}

@Injectable()
export class ManualContentsState extends ContentsStateBase {
    public schema: { name: string };

    constructor(
        appsState: AppsState, contentsService: ContentsService, dialogs: DialogService
    ) {
        super(appsState, contentsService, dialogs);
    }

    public get schemaName() {
        return this.schema.name;
    }
}

function buildStatusQueries(statuses: ReadonlyArray<StatusInfo> | undefined): ReadonlyArray<SavedQuery> {
    return statuses?.map(s => buildStatusQuery(s)) || [];
}

function buildStatusQuery(s: StatusInfo) {
    const query = {
        filter: {
            and: [
                { path: 'status', op: 'eq', value: s.status }
            ]
        }
    };

    return ({ name: s.status, color: s.color, query });
}
