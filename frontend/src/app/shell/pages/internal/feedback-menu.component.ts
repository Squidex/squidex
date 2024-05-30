/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit } from '@angular/core';
import markerSDK, { MarkerSdk } from '@marker.io/browser';
import { UIOptions } from '@app/shared';

@Component({
    standalone: true,
    selector: 'sqx-feedback-menu',
    styleUrls: ['./feedback-menu.component.scss'],
    templateUrl: './feedback-menu.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeedbackMenuComponent implements OnInit, OnDestroy {
    private widget?: MarkerSdk;

    public readonly markerProject = inject(UIOptions).value.markerProject;

    public ngOnDestroy() {
        this.widget?.unload();
    }

    public async ngOnInit() {
        if (!this.markerProject) {
            return;
        }

        this.widget = await markerSDK.loadWidget({ project: this.markerProject });
        this.widget.hide();
    }

    public capture() {
        this.widget?.capture('fullscreen');
    }
}
