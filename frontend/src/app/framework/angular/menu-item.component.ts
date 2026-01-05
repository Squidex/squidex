/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, numberAttribute, Output, TemplateRef, ViewChild } from '@angular/core';
import { ConfirmClickDirective } from './forms/confirm-click.directive';
import { TooltipDirective } from './modals/tooltip.directive';
import { TranslatePipe } from './pipes/translate.pipe';

@Component({
    selector: 'sqx-menu-item',
    styleUrls: ['./menu-item.component.scss'],
    templateUrl: './menu-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        ConfirmClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class MenuItemComponent {
    @Input()
    public label = '';

    @Input()
    public menuLabel = '';

    @Input()
    public icon = '';

    @Input()
    public tooltip = '';

    @Input({ transform: numberAttribute })
    public tabIndex = -1;

    @Input({ transform: booleanAttribute })
    public disabled = false;

    @Input({ transform: booleanAttribute })
    public small = false;

    @Input()
    public confirmRememberKey = '';

    @Input()
    public confirmTitle = '';

    @Input()
    public confirmText = '';

    @Output()
    public action = new EventEmitter();

    @ViewChild('dropdownTemplate', { static: true })
    template!: TemplateRef<any>;

    public get showInDropdown() {
        return this.label || this.menuLabel;
    }
}