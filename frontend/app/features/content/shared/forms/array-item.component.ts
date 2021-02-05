/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { AppLanguageDto, EditContentForm, FieldArrayItemForm, FieldArrayItemValueForm, FieldFormatter, FieldSection, invalid$, NestedFieldDto, StatefulComponent, value$ } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ArraySectionComponent } from './array-section.component';

interface State {
    // The when the section is collapsed.
    isCollapsed: boolean;
}

@Component({
    selector: 'sqx-array-item',
    styleUrls: ['./array-item.component.scss'],
    templateUrl: './array-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArrayItemComponent extends StatefulComponent<State> implements OnChanges {
    @Output()
    public itemRemove = new EventEmitter();

    @Output()
    public itemMove = new EventEmitter<number>();

    @Output()
    public clone = new EventEmitter();

    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public formModel: FieldArrayItemForm;

    @Input()
    public canUnset: boolean;

    @Input()
    public isFirst = false;

    @Input()
    public isLast = false;

    @Input()
    public isDisabled = false;

    @Input()
    public index: number;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(ArraySectionComponent)
    public sections: QueryList<ArraySectionComponent>;

    public isCollapsed = false;
    public isInvalid: Observable<boolean>;

    public title: Observable<string>;

    constructor(changeDetector: ChangeDetectorRef
    ) {
        super(changeDetector, {
            isCollapsed: false
        });
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['formModel']) {
            this.isInvalid = invalid$(this.formModel.form);

            this.title = value$(this.formModel.form).pipe(map(x => this.getTitle(x)));
        }
    }

    private getTitle(value: any) {
        const values: string[] = [];

        for (const field of this.formModel.field.nested) {
            const fieldValue = value[field.name];

            if (fieldValue) {
                const formatted = FieldFormatter.format(field, fieldValue);

                if (formatted) {
                    values.push(formatted);
                }
            }
        }

        return values.join(', ');
    }

    public collapse() {
        this.next({ isCollapsed: true });
    }

    public expand() {
        this.next({ isCollapsed: false });
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

    public trackBySection(_index: number, section: FieldSection<NestedFieldDto, FieldArrayItemValueForm>) {
        return section.separator?.fieldId;
    }
}