/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, ChangeDetectionStrategy, Component, computed, ElementRef, forwardRef, input, signal, viewChild } from '@angular/core';
import { FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { getTagValues, MathHelper, SignalControlComponent, TagValue, TextMeasurer } from '@app/framework/internal';
import { ResizedDirective } from '../../resized.directive';

export const SQX_RADIO_GROUP_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => RadioGroupComponent), multi: true,
};

@Component({
    selector: 'sqx-radio-group',
    styleUrls: ['./radio-group.component.scss'],
    templateUrl: './radio-group.component.html',
    providers: [
        SQX_RADIO_GROUP_CONTROL_VALUE_ACCESSOR,
    ],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        FormsModule,
        ResizedDirective,
    ],
})
export class RadioGroupComponent extends SignalControlComponent<string> {
    private readonly containerElement = viewChild<ElementRef<HTMLDivElement>>('container');
    private readonly containerWidth = signal(0);
    private readonly textMeasurer = new TextMeasurer(() => this.containerElement());

    protected readonly controlId = MathHelper.guid();

    /** Renders the options in a single line, in multiple lines or depending on the available space. */
    public readonly layout = input<'Auto' | 'Singleline' | 'Multiline'>('Auto');

    /** Keeps the order of the values instead of sorting them by name. */
    public readonly unsorted = input(true, { transform: booleanAttribute });

    /** The values to choose from. */
    public readonly values = input<ReadonlyArray<string | TagValue>>([]);

    private readonly tagValuesUnsorted = computed(() => {
        return getTagValues(this.values(), false);
    });

    private readonly tagValuesSorted = computed(() => {
        return this.tagValuesUnsorted().sortedByString(x => x.lowerCaseName);
    });

    protected readonly tagValues = computed(() => {
        return this.unsorted() ? this.tagValuesUnsorted() : this.tagValuesSorted();
    });

    private readonly tagValuesWidth = computed(() => {
        let width = 0;
        for (const value of this.tagValuesUnsorted()) {
            width += 40;
            width += this.textMeasurer.getTextSize(value.name);
        }

        return width;
    });

    protected readonly isSingleline = computed(() => {
        const layout = this.layout();
        if (layout !== 'Auto') {
            return layout === 'Singleline';
        }

        const width = this.tagValuesWidth();
        if (width < 0) {
            return false;
        }

        return width < this.containerWidth();
    });

    protected readonly valueModel = signal<any>(undefined);

    protected updateContainerWidth(width: number) {
        this.containerWidth.set(width);
    }

    public writeValue(obj: any) {
        this.valueModel.set(obj);
    }
}