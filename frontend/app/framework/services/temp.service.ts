/*
* Squidex Headless CMS
*
* @license
* Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
*/

import { Injectable } from '@angular/core';

export const TempServiceFactory = () => {
   return new TempService();
};

@Injectable()
export class TempService {
   private value: any = null;

   public put(value: any) {
       this.value = value;
   }

   public fetch() {
       const result = this.value;

       this.value = null;

       return result;
   }
}