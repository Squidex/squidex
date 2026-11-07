/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, effect, input, model } from '@angular/core';
import { FloatingPlacement, ModalModel } from '@app/framework/internal';
import { DropdownMenuComponent } from './dropdown-menu.component';
import { ModalPlacementDirective } from './modals/modal-placement.directive';
import { ModalDirective } from './modals/modal.directive';
import { TooltipDirective } from './modals/tooltip.directive';

export interface Language { iso2Code: string; englishName: string; isMasterLanguage?: boolean }

@Component({
    selector: 'sqx-language-selector',
    styleUrls: ['./language-selector.component.scss'],
    templateUrl: './language-selector.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        DropdownMenuComponent,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
    ],
})
export class LanguageSelectorComponent<T extends Language = Language> {
    /** The selected language. Falls back to the master or first language when not part of the languages. */
    public readonly language = model.required<T>();

    /** The languages to choose from. */
    public readonly languages = input.required<ReadonlyArray<T>>();

    /** Shows the selector even if there is only one language. */
    public readonly showAlways = input(false, { transform: booleanAttribute });

    /** Marks languages as missing when their value is false. */
    public readonly exists = input<{ [language: string]: boolean } | null>();

    /** The translation progress per language in percent. Renders the selector as dropdown when set. */
    public readonly percents = input<{ [language: string]: number } | null>();

    /** The position of the dropdown relative to the button. */
    public readonly dropdownPosition = input<FloatingPlacement>('bottom-end');

    /** The size of the buttons. */
    public readonly size = input<'sm' | 'md' | 'lg'>('md');

    protected readonly dropdown = new ModalModel();

    constructor() {
        effect(() => {
            const languages = this.languages();
            if (!languages || languages.length === 0) {
                return;
            }

            const language = this.language();
            if (language && languages.includes(language)) {
                return;
            }

            this.selectLanguage(languages.find(l => l.isMasterLanguage) || languages[0]);
        });
    }

    protected selectLanguage(language: T) {
        if (language?.iso2Code !== this.language()?.iso2Code) {
            this.language.set(language);
        }
    }
}
