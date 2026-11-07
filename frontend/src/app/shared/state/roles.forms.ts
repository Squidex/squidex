/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { computed } from '@angular/core';
import { applyEach, maxLength, required } from '@angular/forms/signals';
import { SignalForm } from '@app/framework';
import { AddRoleDto, RoleDto, UpdateRoleDto } from '../model';

type EditRoleFormModel = { permissions: string[] };

export class EditRoleForm extends SignalForm<EditRoleFormModel, UpdateRoleDto, RoleDto> {
    constructor() {
        super({ permissions: [] }, path => {
            applyEach(path.permissions, permission => {
                required(permission);
            });
        });
    }

    public addPermission() {
        this.model.update(x => ({ ...x, permissions: [...x.permissions, ''] }));
    }

    public removePermission(index: number) {
        this.model.update(x => ({ ...x, permissions: x.permissions.filter((_, i) => i !== index) }));
    }

    protected transformLoad(value: Partial<RoleDto>): EditRoleFormModel {
        return { permissions: [...(value.permissions || [])] };
    }

    protected transformSubmit(value: EditRoleFormModel) {
        return new UpdateRoleDto({ permissions: value.permissions, properties: {} });
    }
}

export class AddRoleForm extends SignalForm<{ name: string }, AddRoleDto> {
    public readonly hasNoName = computed(() => {
        return !this.model().name;
    });

    constructor() {
        super({ name: '' }, path => {
            required(path.name);
            maxLength(path.name, 40);
        });
    }

    protected transformSubmit(value: { name: string }) {
        return new AddRoleDto(value);
    }
}
