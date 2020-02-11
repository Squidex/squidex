/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, EventEmitter, Input, OnChanges, OnDestroy, Output, QueryList, SimpleChanges, ViewChildren } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { Observable, Subscription } from 'rxjs';

import {
    AppLanguageDto,
    EditContentForm,
    FieldDto,
    FieldFormatter,
    invalid$,
    RootFieldDto,
    value$
} from '@app/shared';

import { FieldEditorComponent } from './field-editor.component';

type FieldControl = { field: FieldDto, control: AbstractControl };

@Component({
    selector: 'sqx-array-item',
    styleUrls: ['./array-item.component.scss'],
    templateUrl: './array-item.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ArrayItemComponent implements OnChanges, OnDestroy {
    private subscription: Subscription;

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
    public editors: QueryList<FieldEditorComponent>;

    public isHidden = false;
    public isInvalid: Observable<boolean>;

    public title: string;

    public fieldControls: ReadonlyArray<FieldControl> = [];

    constructor(
        private readonly changeDetector: ChangeDetectorRef
    ) {
    }

    public ngOnDestroy() {
        this.unsubscribeFromForm();
    }

    private unsubscribeFromForm() {
        if (this.subscription) {
            this.subscription.unsubscribe();
        }
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['itemForm']) {
            this.isInvalid = invalid$(this.itemForm);

            this.unsubscribeFromForm();

            this.subscription =
                value$(this.itemForm)
                    .subscribe(() => {
                        this.updateTitle();
                    });
        }

        if (changes['itemForm'] || changes['field']) {
            this.updateFields();
            this.updateTitle();
        }
    }

    private updateFields() {
        const fields: FieldControl[] = [];

        for (let field of this.field.nested) {
            const control = this.itemForm.get(field.name)!;

            if (control || this.field.properties.isContentField) {
                fields.push({ field, control });
            }
        }

        this.fieldControls = fields;
    }

    private updateTitle() {
        const values: string[] = [];

        for (let { control, field } of this.fieldControls) {
            const formatted = FieldFormatter.format(field, control.value);

            if (formatted) {
                values.push(formatted);
            }
        }

        this.title = values.join(', ');
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
        this.editors.forEach(editor => {
            editor.reset();
        });
    }

    public trackByField(index: number, control: FieldControl) {
        return control.field.name;
    }
}