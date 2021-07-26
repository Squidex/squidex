/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnChanges, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { AppLanguageDto, ComponentFieldPropertiesDto, ComponentForm, EditContentForm, fadeAnimation, FieldDto, FieldSection, ModalModel, ResourceOwner, SchemaDto, Types } from '@app/shared';
import { ComponentSectionComponent } from './component-section.component';

@Component({
    selector: 'sqx-component[form][formContext][formModel][language][languages]',
    styleUrls: ['./component.component.scss'],
    templateUrl: './component.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [
        fadeAnimation,
    ],
})
export class ComponentComponent extends ResourceOwner implements OnChanges {
    @Input()
    public canUnset?: boolean | null;

    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formModel: ComponentForm;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ComponentSectionComponent)
    public sections: QueryList<ComponentSectionComponent>;

    public schemasDropdown = new ModalModel();
    public schemasList: ReadonlyArray<SchemaDto>;

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
    ) {
        super();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            this.unsubscribeAll();

            this.own(
                this.formModel.form.valueChanges
                    .subscribe(() => {
                        this.changeDetector.detectChanges();
                    }));

            if (Types.is(this.formModel.field.properties, ComponentFieldPropertiesDto)) {
                this.schemasList = this.formModel.field.properties.schemaIds?.map(x => this.formModel.globals.schemas[x]).filter(x => !!x) || [];
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
