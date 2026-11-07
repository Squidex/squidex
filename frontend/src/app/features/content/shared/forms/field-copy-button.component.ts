/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input, linkedSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { form, FormField } from '@angular/forms/signals';
import { AppLanguageDto, CheckboxGroupComponent, DropdownMenuComponent, FieldForm, ModalDirective, ModalModel, ModalPlacementDirective, SignalSelectDirective, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-field-copy-button',
    styleUrls: ['./field-copy-button.component.scss'],
    templateUrl: './field-copy-button.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CheckboxGroupComponent,
        DropdownMenuComponent,
        FormField,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        SignalSelectDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class FieldCopyButtonComponent {
    /** The form model of the field to copy the values for. */
    public readonly formModel = input.required<FieldForm>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    protected readonly copySource = linkedSignal<string | undefined>(() => {
        return this.languages()[0]?.iso2Code;
    });

    protected readonly copySourceForm = form(this.copySource);

    protected readonly copyTargets = linkedSignal<ReadonlyArray<string>>(() => {
        this.copySource();

        return [];
    });

    protected readonly languageCodes = computed(() => {
        const copySource = this.copySource();
        const codes = this.languages().map(x => x.iso2Code);
        if (!copySource) {
            return codes;
        }

        return codes.removed(copySource);
    });

    protected readonly isLocalized = computed(() => {
        return this.formModel().field.isLocalizable && this.languages().length > 1;
    });

    protected readonly dropdown = new ModalModel();

    protected copy() {
        const copySource = this.copySource();
        const copyTargets = this.copyTargets();
        if (!copySource || copyTargets.length === 0) {
            return;
        }

        const formModel = this.formModel();
        const source = formModel.get(copySource).value();

        for (const target of copyTargets) {
            if (target !== copySource) {
                formModel.get(target)?.setValue(source);
            }
        }
    }
}
