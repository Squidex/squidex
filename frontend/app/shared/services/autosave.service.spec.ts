/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { AutoSaveService, LocalStoreService, Version } from '@app/shared/internal';
import { IMock, It, Mock, Times } from 'typemoq';

describe('AutoSaveService', () => {
    let localStore: IMock<LocalStoreService>;

    let autoSaveService: AutoSaveService;

    beforeEach(() => {
        localStore = Mock.ofType<LocalStoreService>();

        autoSaveService = new AutoSaveService(localStore.object);
    });

    it('should remove unsaved created content', () => {
        autoSaveService.remove({ schemaId: '1', schemaVersion: new Version('2') });

        expect().nothing();

        localStore.verify(x => x.remove('autosave.1-2'), Times.once());
    });

    it('should remove unsaved edited content', () => {
        autoSaveService.remove({ schemaId: '1', schemaVersion: new Version('2'), contentId: '3' });

        expect().nothing();

        localStore.verify(x => x.remove('autosave.1-2.3'), Times.once());
    });

    it('should not remove content if key is not defined', () => {
        autoSaveService.remove(null!);

        expect().nothing();

        localStore.verify(x => x.remove(It.isAnyString()), Times.never());
    });

    it('should save unsaved created content', () => {
        autoSaveService.set({ schemaId: '1', schemaVersion: new Version('2') }, { text: 'Hello' });

        expect().nothing();

        localStore.verify(x => x.set('autosave.1-2', '{"text":"Hello"}'), Times.once());
    });

    it('should save unsaved edited content', () => {
        autoSaveService.set({ schemaId: '1', schemaVersion: new Version('2'), contentId: '3' }, { text: 'Hello' });

        expect().nothing();

        localStore.verify(x => x.set('autosave.1-2.3', '{"text":"Hello"}'), Times.once());
    });

    it('should not save content if key is not defined', () => {
        autoSaveService.set(null!, { text: 'Hello' });

        expect().nothing();

        localStore.verify(x => x.set(It.isAnyString(), It.isAnyString()), Times.never());
    });

    it('should not save content if content is not defined', () => {
        autoSaveService.set({ schemaId: '1', schemaVersion: new Version('2') }, null!);

        expect().nothing();

        localStore.verify(x => x.set(It.isAnyString(), It.isAnyString()), Times.never());
    });

    it('should get unsaved created content and delete', () => {
        localStore.setup(x => x.get('autosave.1-2'))
            .returns(() => '{"text":"Hello"}');

        const key = { schemaId: '1', schemaVersion: new Version('2') };

        const content = autoSaveService.fetch(key);

        expect(content).toEqual({ text: 'Hello' });

        localStore.verify(x => x.remove(It.isAnyString()), Times.once());
    });

    it('should get unsaved edited content and remove', () => {
        localStore.setup(x => x.get('autosave.1-2.3'))
            .returns(() => '{"text":"Hello"}');

        const key = { schemaId: '1', schemaVersion: new Version('2'), contentId: '3' };

        const content = autoSaveService.fetch(key);

        expect(content).toEqual({ text: 'Hello' });

        localStore.verify(x => x.remove(It.isAnyString()), Times.once());
    });

    it('should not get content if key is not defined', () => {
        autoSaveService.remove(null!);

        const content = autoSaveService.fetch(null!);

        expect(content).toBeNull();

        localStore.verify(x => x.get(It.isAnyString()), Times.never());
    });
});
