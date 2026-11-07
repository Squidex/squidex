/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AsyncPipe } from '@angular/common';
import { booleanAttribute, ChangeDetectionStrategy, Component, computed, effect, input, numberAttribute, output, viewChildren } from '@angular/core';
import { of } from 'rxjs';
import { map } from 'rxjs/operators';
import { AppLanguageDto, ComponentForm, EditContentForm, FieldDto, FieldFormatter, FormHintComponent, IfOnceDirective, invalid$, NestedFieldDto, ObjectFormBase, TooltipDirective, TranslatePipe, Types, valueProjection$ } from '@app/shared';
import { ComponentSectionComponent } from './component-section.component';

@Component({
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
    private readonly sections = viewChildren(ComponentSectionComponent);

    /** Emits when the user removes the item. */
    public readonly itemRemove = output();

    /** Emits the new index when the user moves the item. */
    public readonly itemMove = output<number>();

    /** Emits when the item has been expanded or collapsed and therefore changed its size. */
    public readonly itemExpanded = output();

    /** Emits when the user clones the item. */
    public readonly clone = output();

    /** Enables the AI features of the editors. */
    public readonly hasChatBot = input.required<boolean>();

    /** The content form the item belongs to. */
    public readonly form = input.required<EditContentForm>();

    /** Additional context information that is passed to custom editors. */
    public readonly formContext = input.required<any>();

    /** The nesting level of the item. */
    public readonly formLevel = input.required<number, unknown>({ transform: numberAttribute });

    /** The form model of the item. */
    public readonly formModel = input.required<ObjectFormBase>();

    /** Renders the editor next to the editor of another version. */
    public readonly isComparing = input.required<boolean, unknown>({ transform: booleanAttribute });

    /** Collapses the item initially, if the user has not expanded or collapsed it before. */
    public readonly isCollapsedInitial = input(false, { transform: booleanAttribute });

    /** Disables the buttons to move the item up. */
    public readonly isFirst = input(false, { transform: booleanAttribute });

    /** Disables the buttons to move the item down. */
    public readonly isLast = input(false, { transform: booleanAttribute });

    /** Disables all buttons of the item. */
    public readonly isDisabled = input(false, { transform: booleanAttribute });

    /** The index of the item in the array. */
    public readonly index = input.required<number, unknown>({ transform: numberAttribute });

    /** The language of the field values. */
    public readonly language = input.required<AppLanguageDto>();

    /** All languages of the app. */
    public readonly languages = input.required<ReadonlyArray<AppLanguageDto>>();

    public readonly isInvalidForm = computed(() => {
        return invalid$(this.formModel().form);
    });

    public readonly isInvalidComponent = computed(() => {
        const formModel = this.formModel();
        if (!Types.is(formModel, ComponentForm)) {
            return of(false);
        }

        return formModel.schemaChanges.pipe(map(x => !x));
    });

    public readonly title = computed(() => {
        const formModel = this.formModel();

        return valueProjection$(formModel.form, () => getTitle(formModel));
    });

    public readonly isCollapsed = computed(() => {
        return this.formModel().collapsedChanges;
    });

    constructor() {
        effect(() => {
            if (this.isCollapsedInitial() && this.formModel().collapsed === null) {
                this.collapse();
            }
        });
    }

    public collapse() {
        this.formModel().collapse();

        this.itemExpanded.emit();
    }

    public expand() {
        this.formModel().expand();

        this.itemExpanded.emit();
    }

    public moveTop() {
        this.itemMove.emit(0);
    }

    public moveUp() {
        this.itemMove.emit(this.index() - 1);
    }

    public moveDown() {
        this.itemMove.emit(this.index() + 1);
    }

    public moveBottom() {
        this.itemMove.emit(99999);
    }

    public reset() {
        this.sections().forEach(section => {
            section.reset();
        });
    }
}

function getTitle(formModel: ObjectFormBase) {
    const value = formModel.form.value;
    const values: string[] = [];

    let valueLength = 0;

    function addFields(fields: ReadonlyArray<FieldDto | NestedFieldDto>) {
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
    } else if (Types.is(formModel.field, FieldDto) && formModel.field.nested) {
       addFields(formModel.field.nested);
    }

    return values.join(', ');
}
