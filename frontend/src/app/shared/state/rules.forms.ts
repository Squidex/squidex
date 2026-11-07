/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Injector } from '@angular/core';
import { applyEach, FieldTree, maxLength, required, SchemaPathTree } from '@angular/forms/signals';
import { SignalForm, SignalValidators } from '@app/framework';
import { RuleElementDto, RuleElementPropertyDto, RuleTriggerDto } from '../model';

export interface StepBranchModel {
    condition: string;
    nextStepId?: string | null;
}

export type StepPropertyValue = string | number | boolean | null | StepBranchModel[];

export type StepFormModel = Record<string, StepPropertyValue>;

export class StepForm extends SignalForm<StepFormModel, Record<string, any>> {
    public readonly editableProperties: ReadonlyArray<RuleElementPropertyDto>;

    constructor(
        public readonly definition: RuleElementDto,
        public readonly stepType: string,
        injector?: Injector,
    ) {
        super(StepForm.buildModel(definition), path => {
            for (const property of definition.properties) {
                if (property.editor === 'Branches') {
                    const branches = path[property.name] as unknown as SchemaPathTree<StepBranchModel[]>;

                    applyEach(branches, branch => {
                        maxLength(branch.condition, 1000);
                    });
                    continue;
                }

                if (property.isRequired && property.editor !== 'Checkbox') {
                    required(path[property.name]!);
                }
            }
        }, injector);

        this.editableProperties = definition.properties.filter(x => x.editor !== 'None');
    }

    private static buildModel(definition: RuleElementDto) {
        const model: StepFormModel = {};

        for (const property of definition.properties) {
            if (property.editor === 'Branches') {
                model[property.name] = [];
                continue;
            }

            let defaultValue: string | null = null;

            if (property.isRequired && property.options) {
                defaultValue = property.options[0];
            }

            model[property.name] = defaultValue;
        }

        return model;
    }

    public field(name: string) {
        return this.form[name] as FieldTree<string>;
    }

    public checkboxField(name: string) {
        return this.form[name] as FieldTree<boolean>;
    }

    public branches(name: string) {
        return this.form[name] as FieldTree<StepBranchModel[]>;
    }

    public addBranch(name: string) {
        this.model.update(model => ({ ...model, [name]: [...this.getBranches(model, name), { condition: '' }] }));
    }

    public removeBranch(name: string, index: number) {
        this.model.update(model => ({ ...model, [name]: this.getBranches(model, name).filter((_, i) => i !== index) }));
    }

    protected transformLoad(value: Partial<StepFormModel>, base: StepFormModel) {
        const result = super.transformLoad(value, base);

        for (const property of this.definition.properties) {
            if (property.editor === 'Branches') {
                const branches = this.getBranches(result, property.name);

                result[property.name] = branches.map(x => ({ condition: x.condition ?? '', nextStepId: x.nextStepId }));
            }
        }

        return result;
    }

    protected transformSubmit(value: StepFormModel) {
        return { ...value, stepType: this.stepType };
    }

    private getBranches(model: StepFormModel, name: string) {
        const branches = model[name];
        if (!Array.isArray(branches)) {
            return [];
        }

        return branches;
    }
}

export type ConditionTriggerType = 'AssetChanged' | 'Comment' | 'SchemaChanged';

export interface ConditionTriggerModel {
    condition: string;
}

export interface ContentChangedSchemaModel {
    schemaId: string;
    condition: string;
}

export interface ContentChangedTriggerModel {
    handleAll: boolean;
    schemas: ContentChangedSchemaModel[];
    referencedSchemas: ContentChangedSchemaModel[];
}

export interface CronJobTriggerModel {
    cronExpression: string;
    cronTimezone: string | null;
}

export interface UsageTriggerModel {
    limit: number;
    numDays: number;
}

export abstract class TriggerForm<TModel, TType extends string> extends SignalForm<TModel, RuleTriggerDto, TModel> {
    constructor(
        public readonly triggerType: TType,
        initialModel: TModel,
        schema?: (path: SchemaPathTree<TModel>) => void,
        injector?: Injector,
    ) {
        super(initialModel, schema, injector);
    }

    protected transformSubmit(value: TModel) {
        return RuleTriggerDto.fromJSON({ ...value, triggerType: this.triggerType });
    }
}

export class ConditionTriggerForm extends TriggerForm<ConditionTriggerModel, ConditionTriggerType> {
    constructor(triggerType: ConditionTriggerType, injector?: Injector) {
        super(triggerType, { condition: '' }, undefined, injector);
    }
}

export class ContentChangedTriggerForm extends TriggerForm<ContentChangedTriggerModel, 'ContentChanged'> {
    constructor(injector?: Injector) {
        super('ContentChanged', { handleAll: false, schemas: [], referencedSchemas: [] }, path => {
            applyEach(path.schemas, schema => {
                required(schema.schemaId);
            });
            applyEach(path.referencedSchemas, schema => {
                required(schema.schemaId);
            });
        }, injector);
    }

    public addSchema() {
        this.model.update(model => ({ ...model, schemas: [...model.schemas, createSchema()] }));
    }

    public removeSchema(index: number) {
        this.model.update(model => ({ ...model, schemas: model.schemas.filter((_, i) => i !== index) }));
    }

    public addReferencedSchema() {
        this.model.update(model => ({ ...model, referencedSchemas: [...model.referencedSchemas, createSchema()] }));
    }

    public removeReferencedSchema(index: number) {
        this.model.update(model => ({ ...model, referencedSchemas: model.referencedSchemas.filter((_, i) => i !== index) }));
    }

    protected transformLoad(value: Partial<ContentChangedTriggerModel>): ContentChangedTriggerModel {
        return {
            handleAll: value.handleAll ?? false,
            schemas: value.schemas?.map(createSchema) ?? [],
            referencedSchemas: value.referencedSchemas?.map(createSchema) ?? [],
        };
    }
}

export class CronJobTriggerForm extends TriggerForm<CronJobTriggerModel, 'CronJob'> {
    constructor(injector?: Injector) {
        super('CronJob', { cronExpression: '', cronTimezone: '' }, path => {
            required(path.cronExpression);
        }, injector);
    }
}

export class UsageTriggerForm extends TriggerForm<UsageTriggerModel, 'Usage'> {
    constructor(injector?: Injector) {
        super('Usage', { limit: 20000, numDays: 3 }, path => {
            required(path.limit);
            SignalValidators.between(path.numDays, 1, 30);
        }, injector);
    }
}

export type AnyTriggerForm = ConditionTriggerForm | ContentChangedTriggerForm | CronJobTriggerForm | UsageTriggerForm;

export function createTriggerForm(triggerType: string, injector?: Injector): AnyTriggerForm | undefined {
    switch (triggerType) {
        case 'AssetChanged':
        case 'Comment':
        case 'SchemaChanged':
            return new ConditionTriggerForm(triggerType, injector);
        case 'ContentChanged':
            return new ContentChangedTriggerForm(injector);
        case 'CronJob':
            return new CronJobTriggerForm(injector);
        case 'Usage':
            return new UsageTriggerForm(injector);
        default:
            return undefined;
    }
}

function createSchema(source?: Partial<ContentChangedSchemaModel>): ContentChangedSchemaModel {
    return { schemaId: source?.schemaId ?? '', condition: source?.condition ?? '' };
}
