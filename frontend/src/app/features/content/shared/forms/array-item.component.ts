/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, EventEmitter, Input, numberAttribute, Output, QueryList, ViewChildren } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppLanguageDto, ComponentForm, EditContentForm, FieldDto, FieldFormatter, FormHintComponent, IfOnceDirective, invalid$, ObjectFormBase, RootFieldDto, TooltipDirective, TranslatePipe, TypedSimpleChanges, Types, valueProjection$ } from '@app/shared';
import { ComponentSectionComponent } from './component-section.component';

@Component({
    standalone: true,
    selector: 'sqx-array-item',
    styleUrls: ['./array-item.component.scss'],
    templateUrl: './array-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        AsyncPipe,
        ComponentSectionComponent,
        FormHintComponent,
        IfOnceDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ArrayItemComponent {
    @Output()
    public itemRemove = new EventEmitter();

    @Output()
    public itemMove = new EventEmitter<number>();

    @Output()
    public itemExpanded = new EventEmitter<number>();

    @Output()
    public clone = new EventEmitter();

    @Input({ required: true })
    public hasChatBot!: boolean;

    @Input({ required: true })
    public form!: EditContentForm;

    @Input({ required: true })
    public formContext!: any;

    @Input({ required: true, transform: numberAttribute })
    public formLevel!: number;

    @Input({ required: true })
    public formModel!: ObjectFormBase;

    @Input({ required: true, transform: booleanAttribute })
    public isComparing = false;

    @Input({ transform: booleanAttribute })
    public isCollapsedInitial = false;

    @Input({ transform: booleanAttribute })
    public isFirst?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isLast?: boolean | null;

    @Input({ transform: booleanAttribute })
    public isDisabled?: boolean | null;

    @Input({ required: true, transform: numberAttribute })
    public index!: number;

    @Input({ required: true })
    public language!: AppLanguageDto;

    @Input({ required: true })
    public languages!: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ComponentSectionComponent)
    public sections!: QueryList<ComponentSectionComponent>;

    public isInvalidForm?: Observable<boolean>;
    public isInvalidComponent?: Observable<boolean>;

    public title?: Observable<string>;

    public get isCollapsed() {
        return this.formModel.collapsedChanges;
    }

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.formModel) {
            this.isInvalidForm = invalid$(this.formModel.form);

            if (Types.is(this.formModel, ComponentForm)) {
                this.isInvalidComponent = this.formModel.schemaChanges.pipe(map(x => !x));
            } else {
                this.isInvalidComponent = of(false);
            }

            this.title = valueProjection$(this.formModel.form, () => getTitle(this.formModel));
        }

        if (changes.formModel || changes.isCollapsedInitial) {
            if (this.isCollapsedInitial && this.formModel.collapsed === null) {
                this.collapse();
            }
        }
    }

    public collapse() {
        this.formModel.collapse();

        this.itemExpanded.emit();
    }

    public expand() {
        this.formModel.expand();

        this.itemExpanded.emit();
    }

    public moveTop() {
        this.itemMove.emit(0);
    }

    public moveUp() {
        this.itemMove.emit(this.index - 1);
    }

    public moveDown() {
        this.itemMove.emit(this.index + 1);
    }

    public moveBottom() {
        this.itemMove.emit(99999);
    }

    public reset() {
        this.sections.forEach(section => {
            section.reset();
        });
    }
}

function getTitle(formModel: ObjectFormBase) {
    const value = formModel.form.value;
    const values: string[] = [];

    let valueLength = 0;

    function addFields(fields: ReadonlyArray<FieldDto>) {
        for (const field of fields) {
            const fieldValue = value[field.name];

            if (fieldValue) {
                const formatted = FieldFormatter.format(field, fieldValue);

                if (formatted) {
                    values.push(formatted);
                    valueLength += formatted.length;

                    if (valueLength > 30) {
                        break;
                    }
                }
            }
        }
    }

    if (Types.is(formModel, ComponentForm) && formModel.schema) {
        const formatted = formModel.schema.displayName;

        values.push(formatted);
        valueLength += formatted.length;

        addFields(formModel.schema.fields);
    } else if (Types.is(formModel.field, RootFieldDto)) {
       addFields(formModel.field.nested);
    }

    return values.join(', ');
}
