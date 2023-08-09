/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, numberAttribute, QueryList, ViewChildren } from '@angular/core';
import { Observable } from 'rxjs';
import { AppLanguageDto, ComponentFieldPropertiesDto, ComponentForm, disabled$, EditContentForm, FieldDto, FieldSection, ModalModel, ResourceOwner, SchemaDto, TypedSimpleChanges, Types } from '@app/shared';
import { ComponentSectionComponent } from './component-section.component';

@Component({
    selector: 'sqx-component',
    styleUrls: ['./component.component.scss'],
    templateUrl: './component.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ComponentComponent extends ResourceOwner {
    @Input({ transform: booleanAttribute })
    public canUnset?: boolean | null;

    @Input({ required: true })
    public form!: EditContentForm;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true, transform: numberAttribute })
    public formLevel!: number;

    @Input({ required: true })
    public formModel!: ComponentForm;

    @Input({ required: true, transform: booleanAttribute })
    public isComparing = false;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ComponentSectionComponent)
    public sections!: QueryList<ComponentSectionComponent>;

    public schemasDropdown = new ModalModel();
    public schemasList: ReadonlyArray<SchemaDto> = [];

    public isDisabled?: Observable<boolean>;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
    ) {
        super();
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formModel) {
            this.unsubscribeAll();

            this.isDisabled = disabled$(this.formModel.form);

            this.own(
                this.formModel.form.valueChanges
                    .subscribe(() => {
                        this.changeDetector.detectChanges();
                    }));

            if (Types.is(this.formModel.field.properties, ComponentFieldPropertiesDto)) {
                this.schemasList = this.formModel.field.properties.schemaIds?.map(x => this.formModel.globals.schemas[x]).defined() || [];
            }
        }
    }

    public reset() {
        this.sections.forEach(section => {
            section.reset();
        });
    }

    public setSchema(schema: SchemaDto) {
        this.formModel.selectSchema(schema.id);
    }

    public trackBySection(_index: number, section: FieldSection<FieldDto, any>) {
        return section.separator?.fieldId;
    }
}
