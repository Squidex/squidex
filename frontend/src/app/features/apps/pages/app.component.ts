/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AppDto, AvatarComponent, ConfirmClickDirective, DropdownMenuComponent, ModalDirective, ModalModel, ModalPlacementDirective, StopClickDirective, TourStepDirective, TranslatePipe } from '@app/shared';

@Component({
    standalone: true,
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
}
