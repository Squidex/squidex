/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppDto, AvatarComponent, ConfirmClickDirective, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, Settings, StopClickDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-app',
    styleUrls: ['./app.component.scss'],
    templateUrl: './app.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
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
    @Input({ required: true })
    public app!: AppDto;

    @Output()
    public leave = new EventEmitter<AppDto>();

    public dropdown = new ModalModel();

    public get canShowContents() {
        return this.app.canAccessContent;
    }

    public get canShowAssets() {
        return this.app.canReadAssets && this.app.roleProperties[Settings.AppProperties.HIDE_ASSETS] !== true;
    }

    public get canShowSettings() {
        return this.app.roleProperties[Settings.AppProperties.HIDE_SETTINGS] !== true;
    }
}
