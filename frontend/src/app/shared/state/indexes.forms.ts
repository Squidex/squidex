/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { applyEach, required } from '@angular/forms/signals';
import { SignalForm } from '@app/framework';
import { CreateIndexDto, IndexFieldDto, SortOrder } from '@app/shared/internal';

type IndexFieldFormModel = { name: string; order: SortOrder | '' };

export class CreateIndexForm extends SignalForm<IndexFieldFormModel[], CreateIndexDto> {
    constructor() {
        super([], path => {
            applyEach(path, field => {
                required(field.name);
                required(field.order);
            });
        });
    }

    public add() {
        this.model.update(fields => [...fields, { name: '', order: '' }]);
    }

    public remove(index: number) {
        this.model.update(fields => fields.filter((_, i) => i !== index));
    }

    protected transformLoad(value: Partial<CreateIndexDto>): IndexFieldFormModel[] {
        return (value.fields || []).map(({ name, order }) => ({ name, order }));
    }

    protected transformSubmit(value: IndexFieldFormModel[]) {
        return new CreateIndexDto({ fields: value.map(x => new IndexFieldDto({ name: x.name, order: x.order as SortOrder })) });
    }
}
