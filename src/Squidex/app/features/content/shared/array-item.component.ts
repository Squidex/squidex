/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, EventEmitter, Input, OnChanges, Output, SimpleChanges } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { Observable } from 'rxjs';

import {
    AppLanguageDto,
    EditContentForm,
    FieldDto,
    invalid$,
    RootFieldDto
} from '@app/shared';

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

    @Output()
    public toggle = new EventEmitter<boolean>();

    @Input()
    public form: EditContentForm;

    @Input()
    public field: RootFieldDto;

    @Input()
    public isHidden = false;

    @Input()
    public isFirst = false;

    @Input()
    public isLast = false;

    @Input()
    public index: number;

    @Input()
    public itemForm: FormGroup;

    @Input()
    public language: AppLanguageDto;

    @Input()
    public languages: AppLanguageDto[];

    public isInvalid: Observable<boolean>;

    public fieldControls: { field: FieldDto, control: AbstractControl }[];

    public ngOnChanges(changes: SimpleChanges) {
        if (changes['itemForm']) {
            this.isInvalid = invalid$(this.itemForm);
        }

        if (changes['itemForm'] || changes['field']) {
            this.fieldControls = this.field.nested.map(field => ({ field, control: this.itemForm.get(field.name)! })).filter(x => !!x.control);
        }
    }

    public emitToggle(value: boolean) {
        this.toggle.emit(value);
    }

    public emitClone() {
        this.clone.emit();
    }

    public emitRemove() {
        this.remove.emit();
    }

    public emitMoveTop() {
        this.move.emit(0);
    }

    public emitMoveUp() {
        this.move.emit(this.index - 1);
    }

    public emitMoveDown() {
        this.move.emit(this.index + 1);
    }

    public emitMoveBottom() {
        this.move.emit(99999);
    }
}