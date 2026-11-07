/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, computed, effect, inject, input } from '@angular/core';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { distinctUntilChanged, map, switchMap } from 'rxjs';
import { TranslatePipe } from '@app/framework';
import { CodeEditorComponent, ConfigureFieldRulesForm, ConfirmClickDirective, ControlErrorsComponent, FIELD_RULE_ACTIONS, FieldPropertiesDto, SchemaDto, SchemasService, SchemasState } from '@app/shared';

@Component({
    selector: 'sqx-schema-field-rules-form',
    styleUrls: ['./schema-field-rules-form.component.scss'],
    templateUrl: './schema-field-rules-form.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        ConfirmClickDirective,
        ControlErrorsComponent,
        FormsModule,
        ReactiveFormsModule,
        TranslatePipe,
    ],
})
export class SchemaFieldRulesFormComponent {
    private readonly schemasState = inject(SchemasState);

    /** The schema to configure the field rules for. */
    public readonly schema = input.required<SchemaDto>();

    protected readonly editForm = new ConfigureFieldRulesForm();

    protected readonly fieldActions = FIELD_RULE_ACTIONS;
    protected readonly fieldCompletions;

    protected readonly fieldOptions = computed(() => {
        const fieldNames = new Set<string>();
        const fieldTags = new Set<string>();

        const addField = (name: string, properties: FieldPropertiesDto) => {
            fieldNames.add(name);

            if (properties.tags) {
                for (const tag of properties.tags) {
                    fieldTags.add(tag);
                }
            }
        };

        for (const field of this.schema().fields) {
            if (field.properties.isContentField) {
                addField(field.name, field.properties);

                for (const nestedField of field.nested || []) {
                    if (nestedField.properties.isContentField) {
                        addField(`${field.name}.${nestedField.name}`, nestedField.properties);
                    }
                }
            }
        }

        return [
            ...[...fieldNames]
                .sorted()
                .map(value => ({ label: `field: ${value}`, value })),
            ...[...fieldTags]
                .sorted()
                .map(value => ({ label: `tag: ${value}`, value: `tag:${value}` })),
        ];
    });

    protected readonly isEditable = computed(() => {
        return this.schema().canUpdateUrls;
    });

    constructor() {
        const schemasService = inject(SchemasService);
        const schemasState = this.schemasState;

        this.fieldCompletions = toSignal(
            toObservable(this.schema).pipe(
                map(schema => schema.name),
                distinctUntilChanged(),
                switchMap(name => schemasService.getFieldRulesCompletion(schemasState.appName, name))));

        effect(() => {
            const schema = this.schema();

            this.editForm.load(schema);
            this.editForm.setEnabled(schema.canUpdateUrls);
        });
    }

    protected add() {
        this.editForm.add(this.fieldOptions()[0].value);
    }

    protected saveSchema() {
        if (!this.isEditable()) {
            return;
        }

        const value = this.editForm.submit();
        if (!value) {
            return;
        }

        this.schemasState.configureFieldRules(this.schema(), value)
            .subscribe({
                next: () => {
                    this.editForm.submitCompleted({ noReset: true });
                },
                error: error => {
                    this.editForm.submitFailed(error);
                },
            });
    }
}
