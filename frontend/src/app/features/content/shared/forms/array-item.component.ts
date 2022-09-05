/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { Observable, of } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppLanguageDto, ComponentForm, EditContentForm, FieldDto, FieldFormatter, FieldSection, invalid$, ObjectFormBase, RootFieldDto, Types, valueProjection$ } from '@app/shared';
import { ComponentSectionComponent } from './component-section.component';

@Component({
    selector: 'sqx-array-item[form][formContext][formLevel][formModel][index][isComparing][language][languages]',
    styleUrls: ['./array-item.component.scss'],
    templateUrl: './array-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ArrayItemComponent implements OnChanges {
    @Output()
    public itemRemove = new EventEmitter();

    @Output()
    public itemMove = new EventEmitter<number>();

    @Output()
    public itemExpanded = new EventEmitter<number>();

    @Output()
    public clone = new EventEmitter();

    @Input()
    public form!: EditContentForm;

    @Input()
    public formContext!: any;

    @Input()
    public formLevel!: number;

    @Input()
    public formModel!: ObjectFormBase;

    @Input()
    public canUnset?: boolean | null;

    @Input()
    public isComparing = false;

    @Input()
    public isCollapsedInitial = false;

    @Input()
    public isFirst?: boolean | null;

    @Input()
    public isLast?: boolean | null;

    @Input()
    public isDisabled?: boolean | null;

    @Input()
    public index!: number;

    @Input()
    public language!: AppLanguageDto;

    @Input()
    public languages!: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ComponentSectionComponent)
    public sections!: QueryList<ComponentSectionComponent>;

    public isInvalid?: Observable<boolean>;
    public isInvalidComponent?: Observable<boolean>;

    public title?: Observable<string>;

    public get isCollapsed() {
        return this.formModel.collapsedChanges;
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            this.isInvalid = invalid$(this.formModel.form);

            if (Types.is(this.formModel, ComponentForm)) {
                this.isInvalidComponent = this.formModel.schemaChanges.pipe(map(x => !x));
            } else {
                this.isInvalidComponent = of(false);
            }

            this.title = valueProjection$(this.formModel.form, () => getTitle(this.formModel));
        }

        if (changes['formModel'] || changes['isCollapsedInitial']) {
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

    public trackBySection(_index: number, section: FieldSection<FieldDto, any>) {
        return section.separator?.fieldId;
    }
}

function getTitle(formModel: ObjectFormBase) {
    const value = formModel.form.value;
    const values: string[] = [];

    let valueLength = 0;

    if (Types.is(formModel, ComponentForm) && formModel.schema) {
        const formatted = formModel.schema.displayName;

        values.push(formatted);
        valueLength += formatted.length;
    }

    if (Types.is(formModel.field, RootFieldDto)) {
        for (const field of formModel.field.nested) {
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

    return values.join(', ');
}
