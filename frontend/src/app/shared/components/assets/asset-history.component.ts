/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ExternalLinkDirective, FromNowPipe, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppsState, AssetDto, HistoryEventDto, HistoryService } from '@app/shared/internal';
import { HistoryMessagePipe } from '../history/pipes';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { AssetUrlPipe } from './pipes';

interface AssetEvent { event: HistoryEventDto; version: number; canDownload: boolean }

@Component({
    standalone: true,
    selector: 'sqx-asset-history',
    styleUrls: ['./asset-history.component.scss'],
    templateUrl: './asset-history.component.html',
    imports: [
        AssetUrlPipe,
        AsyncPipe,
        ExternalLinkDirective,
        FromNowPipe,
        HistoryMessagePipe,
        TooltipDirective,
        TranslatePipe,
        UserNameRefPipe,
        UserPictureRefPipe,
    ],
})
export class AssetHistoryComponent {
    @Input({ required: true })
    public asset!: AssetDto;

    public assetEvents!: Observable<ReadonlyArray<AssetEvent>>;

    constructor(
        private readonly appsState: AppsState,
        private readonly historyService: HistoryService,
    ) {
    }

    public ngOnChanges() {
        const channel = `assets.${this.asset.id}`;

        this.assetEvents =
            this.historyService.getHistory(this.appsState.appName, channel).pipe(
                map(events => {
                    let version = -1;

                    return events.map(event => {
                        const canDownload =
                            event.eventType === 'AssetUpdatedEventV2' ||
                            event.eventType === 'AssetCreatedEventV2';

                        version++;

                        return { event, version, canDownload };
                    });
                }));
    }
}
