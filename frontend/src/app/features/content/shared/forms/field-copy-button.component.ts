/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, input, linkedSignal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AppLanguageDto, CheckboxGroupComponent, DropdownMenuComponent, FieldForm, ModalDirective, ModalModel, ModalPlacementDirective, TooltipDirective, TranslatePipe } from '@app/shared';

@Component({
    selector: 'sqx-field-copy-button',
    styleUrls: ['./field-copy-button.component.scss'],
    templateUrl: './field-copy-button.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CheckboxGroupComponent,
        DropdownMenuComponent,
        FormsModule,
        ModalDirective,
        ModalPlacementDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class FieldCopyButtonComponent {
    /** The form model of the field to copy the values for. */
    public readonly formModel = input.required<FieldForm>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    public readonly copySource = linkedSignal<string | undefined>(() => {
        return this.languages()[0]?.iso2Code;
    });

    public readonly copyTargets = linkedSignal<ReadonlyArray<string>>(() => {
        this.copySource();

        return [];
    });

    public readonly languageCodes = computed(() => {
        const copySource = this.copySource();
        const codes = this.languages().map(x => x.iso2Code);
        if (!copySource) {
            return codes;
        }

        return codes.removed(copySource);
    });

    public readonly isLocalized = computed(() => {
        return this.formModel().field.isLocalizable && this.languages().length > 1;
    });

    public readonly dropdown = new ModalModel();

    public copy() {
        const copySource = this.copySource();
        const copyTargets = this.copyTargets();
        if (!copySource || copyTargets.length === 0) {
            return;
        }

        const formModel = this.formModel();
        const source = formModel.get(copySource).form.value;

        for (const target of copyTargets) {
            if (target !== copySource) {
                formModel.get(target)?.setValue(source);
            }
        }
    }
}
