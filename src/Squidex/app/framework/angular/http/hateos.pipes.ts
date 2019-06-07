import { Pipe, PipeTransform } from '@angular/core';

import { Resource } from '@app/framework/internal';

@Pipe({
    name: 'sqxHasLink',
    pure: true
})
export class HasLinkPipe implements PipeTransform {
    public transform(value: Resource, rel: string) {
        return value._links && !!value._links[rel];
    }
}

@Pipe({
    name: 'sqxHasNoLink',
    pure: true
})
export class HasNoLinkPipe implements PipeTransform {
    public transform(value: Resource, rel: string) {
        return !value._links || !value._links[rel];
    }
}