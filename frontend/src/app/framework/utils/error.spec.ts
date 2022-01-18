/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { IMock, It, Mock } from 'typemoq';
import { LocalizerService } from './../services/localizer.service';
import { ErrorDto } from './error';

describe('ErrorDto', () => {
    let localizer: IMock<LocalizerService>;

    beforeEach(() => {
        document.title = '';

        localizer = Mock.ofType<LocalizerService>();
        localizer.setup(x => x.getOrKey(It.isAnyString()))
            .returns((key: string) => key.substring(5));
    });

    it('should create simple message with error code', () => {
        const error = new ErrorDto(500, 'i18n:error.', 'ERROR_CODE_XYZ');

        expect(error.errorCode).toBe('ERROR_CODE_XYZ');
    });

    it('should create simple message if no details are specified.', () => {
        const error = new ErrorDto(500, 'i18n:error.');

        const result = error.translate(localizer.object);

        expect(result).toBe('error.');
    });

    it('should append dot to message', () => {
        const error = new ErrorDto(500, 'i18n:error');

        const result = error.translate(localizer.object);

        expect(result).toBe('error.');
    });

    it('should append dot to detail', () => {
        const error = new ErrorDto(500, 'i18n:error.', null, ['i18n:detail']);

        const result = error.translate(localizer.object);

        expect(result).toBe('error.\n\n * detail.\n');
    });

    it('should create html list if detail has one item', () => {
        const error = new ErrorDto(500, 'i18n:error.', null, ['i18n:detail.']);

        const result = error.translate(localizer.object);

        expect(result).toBe('error.\n\n * detail.\n');
    });

    it('should create html list if error has more items.', () => {
        const error = new ErrorDto(500, 'i18n:error.', null, ['i18n:detail1.', 'i18n:detail2.']);

        const result = error.translate(localizer.object);

        expect(result).toBe('error.\n\n * detail1.\n * detail2.\n');
    });
});
