/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injectable } from '@angular/core';
import { forkJoin, Observable, of } from 'rxjs';
import { catchError, distinctUntilChanged, map, switchMap, tap } from 'rxjs/operators';

import {
    DateTime,
    DialogService,
    ErrorDto,
    ImmutableArray,
    Pager,
    ResourceLinks,
    shareSubscribed,
    State,
    Version,
    Versioned
} from '@app/framework';

import { SchemaDto } from './../services/schemas.service';
import { AppsState } from './apps.state';
import { SchemasState } from './schemas.state';

import { ContentDto, ContentQueryStatus, ContentsService } from './../services/contents.service';

interface Snapshot {
    // The current comments.
    contents: ImmutableArray<ContentDto>;

    // The pagination information.
    contentsPager: Pager;

    // The query to filter and sort contents.
    contentsQuery?: string;

    // Indicates if the contents are loaded.
    isLoaded?: boolean;

    // Indicates which status is shown.
    status: ContentQueryStatus;

    // The selected content.
    selectedContent?: ContentDto | null;

    // The links.
    links: ResourceLinks;
}

export const CONTENT_STATUSES = {
    'PublishedDraft': 'Published and Drafts (Default)',
    'PublishedOnly': 'Published only',
    'Archived': 'Archived'
};

function sameContent(lhs: ContentDto, rhs?: ContentDto): boolean {
    return lhs === rhs || (!!lhs && !!rhs && lhs.id === rhs.id && lhs.version === rhs.version);
}

export abstract class ContentsStateBase extends State<Snapshot> {
    public selectedContent =
        this.changes.pipe(map(x => x.selectedContent),
            distinctUntilChanged(sameContent));

    public contents =
        this.changes.pipe(map(x => x.contents),
            distinctUntilChanged());

    public contentsPager =
        this.changes.pipe(map(x => x.contentsPager),
            distinctUntilChanged());

    public contentsQuery =
        this.changes.pipe(map(x => x.contentsQuery),
            distinctUntilChanged());

    public isLoaded =
        this.changes.pipe(map(x => !!x.isLoaded),
            distinctUntilChanged());

    public isArchive =
        this.changes.pipe(map(x => x.status === 'Archived'),
            distinctUntilChanged());

    public status =
        this.changes.pipe(map(x => x.status),
            distinctUntilChanged());

    public links =
        this.changes.pipe(map(x => x.links),
            distinctUntilChanged());

    constructor(
        private readonly appsState: AppsState,
        private readonly contentsService: ContentsService,
        private readonly dialogs: DialogService
    ) {
        super({ contents: ImmutableArray.of(), contentsPager: new Pager(0), status: 'PublishedDraft', links: {} });
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
                this.snapshot.contentsQuery, undefined,
                this.snapshot.status).pipe(
            tap(({ total, items, _links: links }) => {
                if (isReload) {
                    this.dialogs.notifyInfo('Contents reloaded.');
                }

                return this.next(s => {
                    const contents = ImmutableArray.of(items);
                    const contentsPager = s.contentsPager.setCount(total);

                    let selectedContent = s.selectedContent;

                    if (selectedContent) {
                        selectedContent = contents.find(x => x.id === selectedContent!.id) || selectedContent;
                    }

                    return { ...s, contents, contentsPager, selectedContent, isLoaded: true, links };
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

    public publishChanges(content: ContentDto, dueTime: string | null): Observable<ContentDto> {
        return this.contentsService.putStatus(this.appName, content, 'Published', dueTime, content.version).pipe(
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

    public update(content: ContentDto, request: any, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, content, request, false, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public proposeUpdate(content: ContentDto, request: any): Observable<ContentDto> {
        return this.contentsService.putContent(this.appName, content, request, true, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public discardChanges(content: ContentDto, now?: DateTime): Observable<ContentDto> {
        return this.contentsService.discardChanges(this.appName, content, content.version).pipe(
            tap(updated => {
                this.dialogs.notifyInfo('Content updated successfully.');

                this.replaceContent(updated, content.version);
            }),
            shareSubscribed(this.dialogs));
    }

    public patch(content: ContentDto, request: any, now?: DateTime): Observable<ContentDto> {
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
                const selectedContent = s.selectedContent && s.selectedContent.id === content.id ? content : s.selectedContent;

                return { ...s, contents, selectedContent };
            });
        }
    }

    public filterStatus(status: ContentQueryStatus): Observable<any> {
        this.next(s => ({ ...s, contentsPager: new Pager(0), contentsQuery: undefined, status }));

        return this.loadInternal();
    }

    public search(query?: string): Observable<any> {
        this.next(s => ({ ...s, contentsPager: new Pager(0), contentsQuery: query }));

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