/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, Output, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { AppLanguageDto, EditContentForm, FieldFormatter, invalid$, NestedFieldDto, RootFieldDto, value$ } from '@app/shared';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { FieldSection } from './../group-fields.pipe';
import { ArraySectionComponent } from './array-section.component';
import { FieldEditorComponent } from './field-editor.component';

@Component({
    selector: 'sqx-array-item',
    styleUrls: ['./array-item.component.scss'],
    templateUrl: './array-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArrayItemComponent implements OnChanges {
    @Output()
    public remove = new EventEmitter();

    @Output()
    public move = new EventEmitter<number>();

    @Output()
    public clone = new EventEmitter();

    @Input()
    public form: EditContentForm;

    @Input()
    public formContext: any;

    @Input()
    public field: RootFieldDto;

    @Input()
    public isFirst = false;

    @Input()
    public isLast = false;

    @Input()
    public isDisabled = false;

    @Input()
    public index: number;

    @Input()
    public itemForm: FormGroup;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: ReadonlyArray<AppLanguageDto>;

    @ViewChildren(FieldEditorComponent)
    public sections: QueryList<ArraySectionComponent>;

    public isHidden = false;
    public isInvalid: Observable<boolean>;

    public title: Observable<string>;

    constructor(
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['itemForm']) {
            this.isInvalid = invalid$(this.itemForm);
        }

        if (changes['itemForm'] || changes['field']) {
            this.title = value$(this.itemForm).pipe(map(x => this.getTitle(x)));
        }
    }

    private getTitle(value: any) {
        const values: string[] = [];

        for (const field of this.field.nested) {
            const control = this.itemForm.get(field.name);

            if (control) {
                const formatted = FieldFormatter.format(field, control.value);

                if (formatted) {
                    values.push(formatted);
                }
            }
        }

        return values.join(', ');
    }

    public collapse() {
        this.isHidden = true;

        this.changeDetector.markForCheck();
    }

    public expand() {
        this.isHidden = false;

        this.changeDetector.markForCheck();
    }

    public moveTop() {
        this.move.emit(0);
    }

    public moveUp() {
        this.move.emit(this.index - 1);
    }

    public moveDown() {
        this.move.emit(this.index + 1);
    }

    public moveBottom() {
        this.move.emit(99999);
    }

    public reset() {
        this.sections.forEach(section => {
            section.reset();
        });
    }

    public trackBySection(index: number, section: FieldSection<NestedFieldDto>) {
        return section.separator?.fieldId;
    }
}