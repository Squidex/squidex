/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, computed, effect, input, numberAttribute, viewChildren } from '@angular/core';
import { AppLanguageDto, ComponentFieldPropertiesDto, ComponentForm, disabled$, DropdownMenuComponent, EditContentForm, FormHintComponent, ModalDirective, ModalModel, ModalPlacementDirective, SchemaDto, TranslatePipe, Types } from '@app/shared';
import { ComponentSectionComponent } from './component-section.component';

@Component({
    selector: 'sqx-component',
    styleUrls: ['./component.component.scss'],
    templateUrl: './component.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ComponentSectionComponent,
        DropdownMenuComponent,
        FormHintComponent,
        ModalDirective,
        ModalPlacementDirective,
        TranslatePipe,
    ],
})
export class ComponentComponent {
    private readonly sections = viewChildren(ComponentSectionComponent);

    /** Enables the AI features of the editors. */
    public readonly hasChatBot = input.required<boolean>();

    /** The content form the field belongs to. */
    public readonly form = input.required<EditContentForm>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The nesting level of the field, 0 for root fields. */
    public readonly formLevel = input.required<number, unknown>({ transform: numberAttribute });

    /** The form model of the component field. */
    public readonly formModel = input.required<ComponentForm>();

    /** Renders the editors next to the editors of another version. */
    public readonly isComparing = input.required<boolean, unknown>({ transform: booleanAttribute });

    /** The language of the field values. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    public readonly schemasDropdown = new ModalModel();

    public readonly schemasList = computed<ReadonlyArray<SchemaDto>>(() => {
        const formModel = this.formModel();

        const properties = formModel.field.properties;
        if (!Types.is(properties, ComponentFieldPropertiesDto)) {
            return [];
        }

        return properties.schemaIds?.map(x => formModel.globals.schemas[x]).defined() || [];
    });

    public readonly isDisabled = computed(() => {
        return disabled$(this.formModel().form);
    });

    constructor(changeDetector: ChangeDetectorRef) {
        effect(onCleanup => {
            const subscription =
                this.formModel().form.valueChanges
                    .subscribe(() => {
                        changeDetector.detectChanges();
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public reset() {
        for (const section of this.sections()) {
            section.reset();
        }
    }

    public setSchema(schema: SchemaDto) {
        this.formModel().selectSchema(schema.id);
    }
}
