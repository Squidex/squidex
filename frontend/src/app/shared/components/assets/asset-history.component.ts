/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { map } from 'rxjs/operators';
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
    private readonly appsState = inject(AppsState);
    private readonly historyService = inject(HistoryService);

    /** The asset to show the history for. */
    public readonly asset = input.required<AssetDto>();

    protected readonly assetEvents = rxResource({
        params: () => ({ asset: this.asset() }),
        stream: ({ params }) => this.historyService.getHistory(this.appsState.appName, `assets.${params.asset.id}`).pipe(
            map(events => events.map((event, version) => ({ event, version, canDownload: isDownloadable(event) })))),
        defaultValue: [],
    });
}

function isDownloadable(event: HistoryEventDto) {
    return event.eventType === 'AssetUpdatedEventV2' || event.eventType === 'AssetCreatedEventV2';
}
