/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, effect, input, output, signal, untracked } from '@angular/core';
import { UntypedFormGroup } from '@angular/forms';
import { AppLanguageDto, AppSettingsDto, FieldDto, SchemaDto, TranslatePipe } from '@app/shared';
import { JsonMoreComponent } from '../types/json-more.component';
import { FieldFormCommonComponent } from './field-form-common.component';
import { FieldFormUIComponent } from './field-form-ui.component';
import { FieldFormValidationComponent } from './field-form-validation.component';

@Component({
    selector: 'sqx-field-form',
    styleUrls: ['./field-form.component.scss'],
    templateUrl: './field-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FieldFormCommonComponent,
        FieldFormUIComponent,
        FieldFormValidationComponent,
        JsonMoreComponent,
        TranslatePipe,
    ],
})
export class FieldFormComponent {
    /** Emits when the user cancels editing. */
    public readonly dialogClose = output();

    /** Shows the cancel and save buttons. */
    public readonly showButtons = input(false, { transform: booleanAttribute });

    /** Enables the form and the save button, otherwise the form is disabled. */
    public readonly isEditable = input(false, { transform: booleanAttribute });

    /** The form group to edit the field properties. */
    public readonly fieldForm = input.required<UntypedFormGroup>();

    /** The field to edit. */
    public readonly field = input.required<FieldDto>();

    /** The schema the field belongs to. */
    public readonly schema = input.required<SchemaDto>();

    /** The settings of the app. */
    public readonly settings = input.required<AppSettingsDto>();

    /** The languages of the app, used to edit localized default values. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    /** Shows the localized default values, if the field is localizable. */
    public readonly isLocalizable = input(false, { transform: booleanAttribute });

    protected readonly selectedTab = signal(0);

    constructor() {
        effect(() => {
            const fieldForm = this.fieldForm();
            const isEditable = this.isEditable();

            untracked(() => {
                if (!isEditable) {
                    fieldForm.disable();
                } else {
                    fieldForm.enable();
                }
            });
        });
    }

    protected selectTab(tab: number) {
        this.selectedTab.set(tab);
    }
}
