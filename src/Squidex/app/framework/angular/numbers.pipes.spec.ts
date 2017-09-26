/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { FileSizePipe, KNumberPipe } from './..';

describe('FileSizePipe', () => {
    it('should calculate correct human file size', () => {
        const pipe = new FileSizePipe();

        expect(pipe.transform(50)).toBe('50 B');
        expect(pipe.transform(1024)).toBe('1.0 kB');
        expect(pipe.transform(1260000)).toBe('1.2 MB');
    });
});

describe('KNumberPipe', () => {
    it('should calculate correct human string', () => {
        const pipe = new KNumberPipe();

        expect(pipe.transform(0)).toBe('0');
        expect(pipe.transform(-1)).toBe('');
        expect(pipe.transform(50)).toBe('50');
        expect(pipe.transform(1024)).toBe('1k');
        expect(pipe.transform(1260000)).toBe('1260k');
    });
});