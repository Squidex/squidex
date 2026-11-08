/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, computed, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AppDto, Settings, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-left-menu',
    styleUrls: ['./left-menu.component.scss'],
    templateUrl: './left-menu.component.html',
    imports: [
        RouterLink,
        RouterLinkActive,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class LeftMenuComponent {
    /** The selected app, used to decide which menu items are visible. */
    public readonly app = input.required<AppDto>();

    protected readonly isAssetsHidden = computed(() => {
        return this.app().roleProperties[Settings.AppProperties.HIDE_ASSETS] === true;
    });

    protected readonly isSettingsHidden = computed(() => {
        return this.app().roleProperties[Settings.AppProperties.HIDE_SETTINGS] === true;
    });

    protected readonly isSchemasHidden = computed(() => {
        return this.app().roleProperties[Settings.AppProperties.HIDE_SCHEMAS] === true;
    });

    protected readonly isApiHidden = computed(() => {
        return this.app().roleProperties[Settings.AppProperties.HIDE_API] === true;
    });
}
