/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { TestBed } from '@angular/core/testing';
import { AppSettingsDto, EditorDto, PatternDto, UpdateAppSettingsDto } from '../model';
import { EditAppSettingsForm } from './apps.forms';

describe('EditAppSettingsForm', () => {
    let form: EditAppSettingsForm;

    beforeEach(() => {
        form = TestBed.runInInjectionContext(() => new EditAppSettingsForm());
    });

    it('should load settings', () => {
        form.load(createSettings());

        expect(form.model()).toMatchObject({
            patterns: [{ name: 'pattern1', regex: 'regex1', message: '' }],
            editors: [{ name: 'editor1', url: 'url1' }],
            hideScheduler: true,
            hideDateTimeModeButton: true,
        });
    });

    it('should add and remove patterns', () => {
        form.load(createSettings());
        form.addPattern();
        form.removePattern(0);

        expect(form.model().patterns).toEqual([{ name: '', regex: '', message: '' }]);
    });

    it('should add and remove editors', () => {
        form.load(createSettings());
        form.addEditor();
        form.removeEditor(0);

        expect(form.model().editors).toEqual([{ name: '', url: '' }]);
    });

    it('should not submit if added pattern is invalid', () => {
        form.addPattern();

        const result = form.submit();

        expect(result).toBeNull();
        expect(form.form.patterns[0].name().errors().map(x => x.kind)).toEqual(['required']);
    });

    it('should submit settings', () => {
        form.load(createSettings());

        const result = form.submit();

        expect(result).toEqual(new UpdateAppSettingsDto({
            patterns: [new PatternDto({ name: 'pattern1', regex: 'regex1', message: '' })],
            editors: [new EditorDto({ name: 'editor1', url: 'url1' })],
            hideScheduler: true,
            hideDateTimeModeButton: true,
        }));
    });
});

function createSettings() {
    return new AppSettingsDto({
        patterns: [new PatternDto({ name: 'pattern1', regex: 'regex1' })],
        editors: [new EditorDto({ name: 'editor1', url: 'url1' })],
        hideScheduler: true,
        hideDateTimeModeButton: true,
        version: 1,
        _links: {},
    });
}
