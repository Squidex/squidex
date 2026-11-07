/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, input, linkedSignal, numberAttribute, output, signal } from '@angular/core';
import { FormControl, FormsModule, ReactiveFormsModule, ValidatorFn, Validators } from '@angular/forms';
import { Keys } from '@app/framework/internal';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ControlErrorsComponent } from './control-errors.component';
import { FocusOnInitDirective } from './focus-on-init.directive';

@Component({
    selector: 'sqx-editable-title',
    styleUrls: ['./editable-title.component.scss'],
    templateUrl: './editable-title.component.html',
    imports: [
        ControlErrorsComponent,
        FocusOnInitDirective,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class EditableTitleComponent {
    /** Emits the new title when renamed. */
    public readonly inputTitleChange = output<string>();

    /** The title. */
    public readonly inputTitle = input.required<string | undefined | null>();

    /** The maximum length of the title. No limit when 0. */
    public readonly inputTitleLength = input(20, { transform: numberAttribute });

    /** Prevents renaming to an empty title. */
    public readonly inputTitleRequired = input(true, { transform: booleanAttribute });

    /** Prevents renaming. */
    public readonly disabled = input(false, { transform: booleanAttribute });

    /** Shows a button to cancel renaming. */
    public readonly closeButton = input(true, { transform: booleanAttribute });

    /** The size of the title and the buttons. */
    public readonly size = input<'sm' | 'md' | 'lg'>('md');

    /** The text to show when the title is empty. */
    public readonly displayFallback = input('');

    protected readonly inputTitleDisplayed = linkedSignal(() => this.inputTitle());

    protected readonly renaming = signal(false);
    protected readonly renameForm = new FormControl<string>('');

    protected onKeyDown(event: KeyboardEvent) {
        if (Keys.isEscape(event)) {
            this.toggleRename();
        }
    }

    protected toggleRename() {
        if (this.disabled()) {
            return;
        }

        if (!this.renaming()) {
            const validators: ValidatorFn[] = [];

            const maxLength = this.inputTitleLength();
            if (maxLength) {
                validators.push(Validators.maxLength(maxLength));
            }

            if (this.inputTitleRequired()) {
                validators.push(Validators.required);
            }

            this.renameForm.setValidators(validators);
        }

        this.renameForm.setValue(this.inputTitleDisplayed() || '');
        this.renaming.update(x => !x);
    }

    protected rename() {
        if (this.disabled()) {
            return;
        }

        if (this.renameForm.valid) {
            const text = this.renameForm.value || '';

            this.inputTitleChange.emit(text);
            this.inputTitleDisplayed.set(text);
            this.renaming.set(false);
        }
    }
}
