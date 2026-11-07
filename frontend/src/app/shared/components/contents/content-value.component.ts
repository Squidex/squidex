/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { ChangeDetectionStrategy, Component, computed, effect, input, signal } from '@angular/core';
import { LongHoverDirective, ModalDirective, ModalModel, ModalPlacementDirective, StopClickDirective, TooltipDirective, TranslatePipe } from '@app/framework';
import { HtmlValue, TableField, TableSettings, Types } from '@app/shared/internal';

@Component({
    selector: 'sqx-content-value',
    styleUrls: ['./content-value.component.scss'],
    templateUrl: './content-value.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        LongHoverDirective,
        ModalDirective,
        ModalPlacementDirective,
        StopClickDirective,
        TooltipDirective,
        TranslatePipe,
    ],
})
export class ContentValueComponent {
    /** The value to render, either plain or as HTML. */
    public readonly value = input.required<any>();

    /** The field of the value, to decide how to render it. */
    public readonly field = input<TableField>();

    /** The table settings, to wrap long text. */
    public readonly fields = input<TableSettings>();

    public readonly previewModal = new ModalModel();

    public readonly wrapping = signal(false);

    public readonly isString = computed(() => {
        return this.field()?.rootField?.properties.fieldType === 'String';
    });

    public readonly isPlain = computed(() => {
        return !Types.is(this.value(), HtmlValue);
    });

    public readonly title = computed(() => {
        return this.isString() && this.isPlain() ? this.value() : undefined;
    });

    constructor() {
        effect(onCleanup => {
            const fields = this.fields();
            if (!fields) {
                return;
            }

            const fieldName = this.field()?.name;
            const subscription =
                fields.fieldWrappings
                    .subscribe(wrappings => {
                        this.wrapping.set(wrappings[fieldName!]);
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public toggle() {
        const field = this.field();
        if (!field) {
            return;
        }

        this.fields()?.toggleWrapping(field.name);
    }

    public show() {
        const value = this.value();
        if (!Types.is(value, HtmlValue) || !value.preview) {
            return;
        }

        this.previewModal.show();
    }

    public hide() {
        this.previewModal.hide();
    }
}
