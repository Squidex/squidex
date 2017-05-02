/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { ApiUrlConfig, Version } from 'framework';

import { FileHelper } from './file-helper';
import { AssetDto } from './../services/assets.service';

describe('FileHelper', () => {
    it('should calculate correct human file size', () => {
        expect(FileHelper.fileSize(50)).toBe('50 B');
        expect(FileHelper.fileSize(1024)).toBe('1.0 kB');
        expect(FileHelper.fileSize(1260000)).toBe('1.2 MB');
    });

    it('should calculate icon', () => {
        expect(FileHelper.fileIcon('video/mp4')).toBe('/images/asset_video.png');
        expect(FileHelper.fileIcon('application/text')).toBe('/images/asset_generic.png');
        expect(FileHelper.fileIcon('application/msword')).toBe('/images/asset_doc.png');
    });

    it('should calculate file type', () => {
        expect(FileHelper.fileType('video/mp4', 'test.mp4')).toBe('mp4');
        expect(FileHelper.fileType('video/mp4', undefined)).toBe('mp4');
        expect(FileHelper.fileType('application/text', 'test.txt')).toBe('txt');
        expect(FileHelper.fileType('application/text', undefined)).toBe('text');

        expect(FileHelper.fileType('invalid', undefined)).toBeUndefined();
        expect(FileHelper.fileType(undefined, undefined)).toBeUndefined();
    });

    it('should calculate asset info for image asset', () => {
        const asset = new AssetDto('1', undefined, undefined, undefined, undefined, 'File.png', 50, 1, 'image/png', true, 100, 20, undefined);

        expect(FileHelper.assetInfo(asset)).toBe('100x20px, 50 B');
    });

    it('should calculate asset info for text asset', () => {
        const asset = new AssetDto('1', undefined, undefined, undefined, undefined, 'File.txt', 50, 1, 'text/plain', false, 0, 0, undefined);

        expect(FileHelper.assetInfo(asset)).toBe('50 B');
    });

    it('should return empty string for invalid asset', () => {
        expect(FileHelper.assetInfo(undefined)).toBe('');
        expect(FileHelper.assetInfo(null)).toBe('');
    });

    it('should return asset name', () => {
        const asset = new AssetDto('1', undefined, undefined, undefined, undefined, 'File.txt', 50, 1, 'text/plain', false, 0, 0, undefined);

        expect(FileHelper.assetName(asset)).toBe('File.txt');
    });

    it('should return preview url', () => {
        const apiUrl = new ApiUrlConfig('my/');
        const asset = new AssetDto('1', undefined, undefined, undefined, undefined, 'File.txt', 50, 1, 'text/plain', false, 0, 0, new Version('123'));

        expect(FileHelper.assetPreviewUrl(apiUrl, asset)).toBe('my/api/assets/1?version=123');
    });

    it('should return download url', () => {
        const apiUrl = new ApiUrlConfig('my/');
        const asset = new AssetDto('1', undefined, undefined, undefined, undefined, 'File.txt', 50, 1, 'text/plain', false, 0, 0, new Version('123'));

        expect(FileHelper.assetUrl(apiUrl, asset).startsWith('my/api/assets/1?q=')).toBeTruthy();
    });
});