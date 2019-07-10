/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, switchMap, tap } from 'rxjs/operators';

import {
    DialogService,
    ErrorDto,
    ImmutableArray,
    Pager,
    shareSubscribed,
    State,
    Version,
    Versioned
} from '@app/framework';

import { SchemaDto } from './../services/schemas.service';
import { AppsState } from './apps.state';
import { SchemasState } from './schemas.state';

import { ContentDto, ContentsService, StatusInfo } from './../services/contents.service';

interface Snapshot {
    // The current comments.
    contents: ImmutableArray<ContentDto>;

    // The pagination information.
    contentsPager: Pager;

    // The query to filter and sort contents.
    contentsQuery?: string;

    // Indicates if the contents are loaded.
    isLoaded?: boolean;

    // The statuses.
    statuses?: StatusInfo[];

    // The selected content.
    selectedContent?: ContentDto | null;

    // Indicates if the user can create a content.
    canCreate?: boolean;

    // Indicates if the user can create and publish a content.
    canCreateAndPublish?: boolean;
}

function sameContent(lhs: ContentDto, rhs?: ContentDto): boolean {
    return lhs === rhs || (!!lhs && !!rhs && lhs.id === rhs.id && lhs.version === rhs.version);
}

export abstract class ContentsStateBase extends State<Snapshot> {
    public selectedContent =
        this.project(x => x.selectedContent, sameContent);

    public contents =
        this.project(x => x.contents);

    public contentsPager =
        this.project(x => x.contentsPager);

    public contentsQuery =
        this.project(x => x.contentsQuery);

    public isLoaded =
        this.project(x => !!x.isLoaded);

    public canCreate =
        this.project(x => !!x.canCreate);

    public canCreateAndPublish =
        this.project(x => !!x.canCreateAndPublish);

    public canCreateAny =
        this.project(x => !!x.canCreate || !!x.canCreateAndPublish);

    public statusQueries =
        this.project2(x => x.statuses, x => buildQueries(x));

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
        super({ contents: ImmutableArray.of(), contentsPager: new Pager(0) });
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

    public load(isReload = false): Observable<any> {
        if (!isReload) {
            this.resetState();
        }

        return this.loadInternal(isReload);
    }

    private loadInternal(isReload = false) {
        return this.loadInternalCore(isReload).pipe(shareSubscribed(this.dialogs));
    }

    private loadInternalCore(isReload = false) {
        return this.contentsService.getContents(this.appName, this.schemaName,
                this.snapshot.contentsPager.pageSize,
                this.snapshot.contentsPager.skip,
                this.snapshot.contentsQuery, undefined).pipe(
            tap(({ total, items, canCreate, canCreateAndPublish, statuses }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Contents reloaded.');
                }

                return this.next(s => {
                    const contents = ImmutableArray.of(items);
                    const contentsPager = s.contentsPager.setCount(total);

                    statuses = s.statuses || statuses;

                    let selectedContent = s.selectedContent;

                    if (selectedContent) {
                        selectedContent = contents.find(x => x.id === selectedContent!.id) || selectedContent;
                    }

                    return { ...s,
                        canCreate,
                        canCreateAndPublish,
                        contents,
                        contentsPager,
                        isLoaded: true,
                        selectedContent,
                        statuses
                    };
                });
            }));
    }

    public create(request: any, publish: boolean): Observable<ContentDto> {
        return this.contentsService.postContent(this.appName, this.schemaName, request, publish).pipe(
            tap(payload => {
                this.dialogs.notifyInfo('Contents created successfully.');

                return this.next(s => {
                    const contents = s.contents.pushFront(payload);
                    const contentsPager = s.contentsPager.incrementCount();

                    return { ...s, contents, contentsPager };
                });
            }),
            shareSubscribed(this.dialogs));
    }

    public changeManyStatus(contents: ContentDto[], status: string, dueTime: string | null): Observable<any> {
        return forkJoin(
            contents.map(c =>
                this.contentsService.putStatus(this.appName, c, status, dueTime, c.version).pipe(
                    catchError(error => of(error))))).pipe(
            tap(results => {
                const error = results.find(x => x instanceof ErrorDto);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return of(error);
            }),
            switchMap(() => this.loadInternalCore()),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public deleteMany(contents: ContentDto[]): Observable<any> {
        return forkJoin(
            contents.map(c =>
                this.contentsService.deleteContent(this.appName, c, c.version).pipe(
                    catchError(error => of(error))))).pipe(
            tap(results => {
                const error = results.find(x => x instanceof ErrorDto);

                if (error) {
                    this.dialogs.notifyError(error);
                }

                return of(error);
            }),
            switchMap(() => this.loadInternal()),
            shareSubscribed(this.dialogs, { silent: true }));
    }

    public publishDraft(content: ContentDto, dueTime: string | null): Observable<ContentDto> {
        return this.contentsService.publishDraft(this.appName, content, dueTime, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public changeStatus(content: ContentDto, status: string, dueTime: string | null): Observable<ContentDto> {
        return this.contentsService.putStatus(this.appName, content, status, dueTime, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated);
            }),
            shareSubscribed(this.dialogs));
    }

    public update(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public proposeDraft(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.proposeDraft(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public discardDraft(content: ContentDto): Observable<ContentDto> {
        return this.contentsService.discardDraft(this.appName, content, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public patch(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.patchContent(this.appName, content, request, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    private replaceContent(content: ContentDto, oldVersion?: Version) {
        if (!oldVersion || !oldVersion.eq(content.version)) {
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

    public search(contentsQuery?: string): Observable<any> {
        this.next(s => ({ ...s, contentsPager: new Pager(0), contentsQuery }));

        return this.loadInternal();
    }

    public goNext(): Observable<any> {
        this.next(s => ({ ...s, contentsPager: s.contentsPager.goNext() }));

        return this.loadInternal();
    }

    public goPrev(): Observable<any> {
        this.next(s => ({ ...s, contentsPager: s.contentsPager.goPrev() }));

        return this.loadInternal();
    }

    public loadVersion(content: ContentDto, version: Version): Observable<Versioned<any>> {
        return this.contentsService.getVersionData(this.appName, this.schemaName, content.id, version).pipe(
            shareSubscribed(this.dialogs));
    }

    private get appName() {
        return this.appsState.appName;
    }

    protected abstract get schemaName(): string;
}

@Injectable()
export class ContentsState extends ContentsStateBase {
    constructor(appsState: AppsState, contentsService: ContentsService, dialogs: DialogService,
        private readonly schemasState: SchemasState
    ) {
        super(appsState, contentsService, dialogs);
    }

    protected get schemaName() {
        return this.schemasState.schemaName;
    }
}

@Injectable()
export class ManualContentsState extends ContentsStateBase {
    public schema: SchemaDto;

    constructor(
        appsState: AppsState, contentsService: ContentsService, dialogs: DialogService
    ) {
        super(appsState, contentsService, dialogs);
    }

    protected get schemaName() {
        return this.schema.name;
    }
}

export type ContentQuery =  { color: string; name: string; filter: string; };

function buildQueries(statuses: StatusInfo[] | undefined): ContentQuery[] {
    return statuses ? statuses.map(s => buildQuery(s)) : [];
}

function buildQuery(s: StatusInfo) {
    return ({ name: s.status, color: s.color, filter: `$filter=status eq '${s.status}'` });
}
