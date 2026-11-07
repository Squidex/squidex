/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, DestroyRef, inject, input, numberAttribute, output, TemplateRef, viewChild } from '@angular/core';
import { LocalizerService } from '../internal';
import { ConfirmClickDirective } from './forms/confirm-click.directive';
import { MenuItemRegistry } from './menu.component';
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
    private readonly destroyRef = inject(DestroyRef);
    private readonly localizerService = inject(LocalizerService);
    private readonly menuItemRegistry = inject(MenuItemRegistry, { optional: true });

    /** The text of the button. */
    public readonly label = input('');

    /** The text in the overflow dropdown. Falls back to the label. */
    public readonly menuLabel = input('');

    /** The name of the icon, without the "icon-" prefix. */
    public readonly icon = input('');

    /** The tooltip of the button. */
    public readonly tooltip = input('');

    /** The tab index of the button. */
    public readonly tabIndex = input(-1, { transform: numberAttribute });

    /** Disables the button. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Renders a small button. */
    public readonly small = input(false, { transform: booleanAttribute });

    /** The key to remember the confirmation, so that it is only asked once. */
    public readonly confirmRememberKey = input('');

    /** The title of the confirmation dialog. No confirmation is needed when empty. */
    public readonly confirmTitle = input('');

    /** The text of the confirmation dialog. */
    public readonly confirmText = input('');

    /** Emits when the button is clicked and the action is confirmed. */
    public readonly action = output();

    public readonly template = viewChild.required<TemplateRef<any>>('dropdownTemplate');

    public readonly actualMenuLabel = computed(() => {
        const key = this.menuLabel() || this.label();
        if (!key) {
            return '';
        }

        return this.localizerService.getOrKey(key);
    });

    constructor() {
        this.menuItemRegistry?.registerItem(this);

        this.destroyRef.onDestroy(() => {
            this.menuItemRegistry?.unregisterItem(this);
        });
    }
}
