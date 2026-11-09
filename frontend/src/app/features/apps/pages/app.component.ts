/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppDto, AvatarComponent, ConfirmClickDirective, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, Settings, StopClickDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html',
    imports: [
        AvatarComponent,
        ConfirmClickDirective,
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        RouterLink,
        StopClickDirective,
        TourStepDirective,
        TranslatePipe,
    ],
})
export class AppComponent {
    /** The app to show in the card. */
    public readonly app = input.required<AppDto>();

    /** Emits the app when the user confirms to leave it. */
    public readonly leave = output<AppDto>();

    protected readonly canShowContents = computed(() => {
        return this.app().canAccessContent;
    });

    protected readonly canShowAssets = computed(() => {
        const app = this.app();

        return app.canReadAssets && app.roleProperties[Settings.AppProperties.HIDE_ASSETS] !== true;
    });

    protected readonly canShowSettings = computed(() => {
        return this.app().roleProperties[Settings.AppProperties.HIDE_SETTINGS] !== true;
    });

    protected readonly dropdown = new ModalModel();
}
