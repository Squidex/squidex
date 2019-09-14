/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, EventEmitter, Input, Output } from '@angular/core';

import { Queries } from '@app/shared/internal';
import { SavedQuery } from '../state/queries';

@Component({
    selector: 'sqx-shared-queries',
    template: `
        <div class="sidebar-section">
            <h3>Shared queries</h3>

            <ng-container *ngIf="queries.queriesShared | async; let queries">
                <ng-container *ngIf="queries.length > 0; else noQuery">
                    <a class="sidebar-item" *ngFor="let saved of queries; trackBy: trackByQuery" (click)="search.emit(saved)"
                        [class.active]="isSelectedQuery(saved)">

                        {{saved.name}}

                        <a class="sidebar-item-remove float-right" (click)="queries.remove(saved)" sqxStopClick>
                            <i class="icon-close"></i>
                        </a>
                    </a>
                </ng-container>

                <ng-template #noQuery>
                    <div class="sidebar-item text-muted">
                        Search for {{types}} and use <i class="icon-star-empty"></i> icon in search form to save query for all contributors.
                    </div>
                </ng-template>
            </ng-container>
        </div>

        <hr />

        <div class="sidebar-section">
            <h3>My queries</h3>

            <ng-container *ngIf="queries.queriesUser | async; let queries">
                <ng-container *ngIf="queries.length > 0; else noQuery">
                    <a class="sidebar-item" *ngFor="let saved of queries; trackBy: trackByQuery" (click)="search.emit(saved)"
                        [class.active]="isSelectedQuery(saved)">

                        {{saved.name}}

                        <a class="sidebar-item-remove float-right" (click)="queries.removeUser(saved)" sqxStopClick>
                            <i class="icon-close"></i>
                        </a>
                    </a>
                </ng-container>

                <ng-template #noQuery>
                    <div class="sidebar-item text-muted">
                        Search for {{types}} and use <i class="icon-star-empty"></i> icon in search form to save query for yourself.
                    </div>
                </ng-template>
            </ng-container>
        </div>
    `
})
export class SavedQueriesComponent {
    @Input()
    public queries: Queries;

    @Input()
    public types: string;

    @Input()
    public queryUsed: (saved: SavedQuery) => boolean;

    @Output()
    public search = new EventEmitter<SavedQuery>();

    public isSelectedQuery(saved: SavedQuery) {
        return this.queryUsed && this.queryUsed(saved);
    }

    public trackByQuery(index: number, query: { name: string }) {
        return query.name;
    }
}