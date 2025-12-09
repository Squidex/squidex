/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights r vbeserved
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, HostBinding, Input } from '@angular/core';
import { AbstractControl } from '@angular/forms';
import { Types } from '@app/framework/internal';
import { MarkdownDirective } from '../markdown.directive';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ControlErrorsComponent } from './control-errors.component';
import { FormAlertComponent } from './form-alert.component';
import { FormHintComponent } from './form-hint.component';

@Component({
    selector: 'sqx-form-row',
    styleUrls: ['./form-row.component.scss'],
    templateUrl: './form-row.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    host: {
        'class': 'form-group',
        '[class.d-block]': 'vertical',
        '[class.row]': '!vertical',
    },
    imports: [
        ControlErrorsComponent,
        FormAlertComponent,
        FormHintComponent,
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class FormRowComponent {
    @Input({ required: true })
    public for!: string | AbstractControl;

    @Input()
    public label?: string;

    @Input()
    public unit?: string;

    @Input({ transform: booleanAttribute })
    public showUnit = false;

    @Input()
    public submitCount: null | number | undefined;

    @Input()
    public formId?: string;

    @Input()
    public hint?: string;

    @Input()
    public alert?: string;

    @Input({ transform: booleanAttribute })
    public hideError = false;

    @Input({ transform: booleanAttribute })
    public check = false;

    @Input({ transform: booleanAttribute })
    public vertical = false;

    @Input({ transform: booleanAttribute })
    public required = false;

    @Input()
    public prefix: any = '';

    @Input()
    @HostBinding('class')
    public class = '';

    protected fieldName = '';

    protected get hasUnit() {
        return !!this.unit || this.showUnit;
    }

    public ngOnChanges() {
        let name = '';
        if (Types.isString(this.for)) {
            name = this.for;
        } else if (this.formId) {
            name = this.formId;
        } else {
            throw new Error('Form id is required.');
        }

        if (this.prefix) {
            name = `${this.prefix}_${name}`;
        }

        this.fieldName = name;
    }
}