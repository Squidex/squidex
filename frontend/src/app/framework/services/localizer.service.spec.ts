/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { LocalizerService } from './localizer.service';

describe('LocalizerService', () => {
    const translations = {
        simple: 'Simple Result',
        withLowerVar: 'Var: {var|lower}.',
        withUpperVar: 'Var: {var|upper}.',
        withMultiple: 'Text1: {text1}, Text2: {Text2}.',
        withVar: 'Var: {var}.',
    };

    it('should instantiate', () => {
        const titleService = new LocalizerService(translations);

        expect(titleService).toBeDefined();
    });

    it('should return key if not found', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.getOrKey('key');

        expect(result).toEqual('key');
    });

    it('should return null if not found', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.get('key');

        expect(result).toBeNull();
    });

    it('should return simple key', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.get('simple');

        expect(result).toEqual('Simple Result');
    });

    it('should return simple key with prefix', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.get('i18n:simple');

        expect(result).toEqual('Simple Result');
    });

    it('should return text with variable', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.get('withVar', { var: 5 });

        expect(result).toEqual('Var: 5.');
    });

    it('should return text with lower variable', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.get('withLowerVar', { var: 'Lower' });

        expect(result).toEqual('Var: lower.');
    });

    it('should return text with upper variable', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.get('withUpperVar', { var: 'upper' });

        expect(result).toEqual('Var: Upper.');
    });

    it('should return text with multiple variables', () => {
        const localizer = new LocalizerService(translations);

        const result = localizer.get('withMultiple', { Text1: 'Hello', Text2: 'World' });

        expect(result).toEqual('Text1: Hello, Text2: World.');
    });
});
