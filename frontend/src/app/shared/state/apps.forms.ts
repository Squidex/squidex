/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { applyEach, maxLength, required } from '@angular/forms/signals';
import { SignalForm, SignalValidators } from '@app/framework';
import { AppDto, AppSettingsDto, CreateAppDto, EditorDto, PatternDto, TransferToTeamDto, UpdateAppDto, UpdateAppSettingsDto } from '../model';

export class CreateAppForm extends SignalForm<{ name: string }, CreateAppDto> {
    constructor() {
        super({ name: '' }, path => {
            required(path.name);
            maxLength(path.name, 40);
            SignalValidators.patternWithMessage(path.name, '[a-z0-9]+(\-[a-z0-9]+)*', 'i18n:apps.appNameValidationMessage');
        });
    }

    protected transformSubmit(value: { name: string }) {
        return new CreateAppDto(value);
    }
}

export class TransferAppForm extends SignalForm<{ teamId: string | null }, TransferToTeamDto, AppDto> {
    constructor() {
        super({ teamId: null });
    }

    protected transformSubmit(value: { teamId: string | null }) {
        return new TransferToTeamDto({ teamId: value.teamId ?? undefined });
    }
}

export class UpdateAppForm extends SignalForm<{ label: string; description: string }, UpdateAppDto, AppDto> {
    constructor() {
        super({ label: '', description: '' }, path => {
            maxLength(path.label, 40);
            maxLength(path.description, 100);
        });
    }

    protected transformSubmit(value: { label: string; description: string }) {
        return new UpdateAppDto(value);
    }
}

export type EditAppSettingsFormModel = {
    patterns: { name: string; regex: string; message: string }[];
    editors: { name: string; url: string }[];
    hideScheduler: boolean;
    hideDateTimeModeButton: boolean;
};

export class EditAppSettingsForm extends SignalForm<EditAppSettingsFormModel, UpdateAppSettingsDto, AppSettingsDto> {
    constructor() {
        super({ patterns: [], editors: [], hideScheduler: false, hideDateTimeModeButton: false }, path => {
            applyEach(path.patterns, pattern => {
                required(pattern.name);
                maxLength(pattern.name, 1000);
                required(pattern.regex);
                maxLength(pattern.regex, 1000);
                maxLength(pattern.message, 1000);
            });

            applyEach(path.editors, editor => {
                required(editor.name);
                maxLength(editor.name, 1000);
                required(editor.url);
                maxLength(editor.url, 1000);
            });
        });
    }

    public addPattern() {
        this.model.update(x => ({ ...x, patterns: [...x.patterns, { name: '', regex: '', message: '' }] }));
    }

    public removePattern(index: number) {
        this.model.update(x => ({ ...x, patterns: x.patterns.filter((_, i) => i !== index) }));
    }

    public addEditor() {
        this.model.update(x => ({ ...x, editors: [...x.editors, { name: '', url: '' }] }));
    }

    public removeEditor(index: number) {
        this.model.update(x => ({ ...x, editors: x.editors.filter((_, i) => i !== index) }));
    }

    protected transformLoad(value: Partial<AppSettingsDto>): EditAppSettingsFormModel {
        return {
            patterns: (value.patterns || []).map(x => ({ name: x.name, regex: x.regex, message: x.message || '' })),
            editors: (value.editors || []).map(x => ({ name: x.name, url: x.url })),
            hideScheduler: value.hideScheduler || false,
            hideDateTimeModeButton: value.hideDateTimeModeButton || false,
        };
    }

    protected transformSubmit(value: EditAppSettingsFormModel) {
        const { editors, patterns, ...other } = value;

        return new UpdateAppSettingsDto({
            ...other,
            editors: editors.map(x =>
                new EditorDto(x)),
            patterns: patterns.map(x =>
                new PatternDto(x)),
        });
    }
}
