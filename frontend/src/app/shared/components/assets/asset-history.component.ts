/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { map, switchMap } from 'rxjs/operators';
import { ExternalLinkDirective, FromNowPipe, TooltipDirective, TranslatePipe } from '@app/framework';
import { AppsState, AssetDto, HistoryEventDto, HistoryService } from '@app/shared/internal';
import { HistoryMessagePipe } from '../history/pipes';
import { UserNameRefPipe, UserPictureRefPipe } from '../pipes';
import { AssetUrlPipe } from './pipes';

@Component({
    selector: 'sqx-asset-history',
    styleUrls: ['./asset-history.component.scss'],
    templateUrl: './asset-history.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AssetUrlPipe,
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
    /** The asset to show the history for. */
    public readonly asset = input.required<AssetDto>();

    protected readonly assetEvents;

    constructor(appsState: AppsState, historyService: HistoryService) {
        this.assetEvents = toSignal(
            toObservable(this.asset).pipe(
                switchMap(asset => historyService.getHistory(appsState.appName, `assets.${asset.id}`)),
                map(events => events.map((event, version) => ({ event, version, canDownload: isDownloadable(event) })))),
            { initialValue: [] });
    }
}

function isDownloadable(event: HistoryEventDto) {
    return event.eventType === 'AssetUpdatedEventV2' || event.eventType === 'AssetCreatedEventV2';
}
