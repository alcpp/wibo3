using DotNetWikiBot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace botSolution
{
    class NoUkInterwikisPublisherFromRu : NoUkInterwikisPublisher
    {
        public NoUkInterwikisPublisherFromRu(NoUkInterwikis nui): base(nui)
        {
            mainPageNamePart = @"Вікіпедія:Статті рувікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті рувікі про Україну без українських інтервік|{0}";
        }

        public override void PublishCategories(PageList plRu)
        {
            /*
            AddFromCategory(plRu, "Музыка Украины", "Музика України");
            AddFromCategory(plRu, "Авиация Украины", "Авіація України");
            
            AddFromCategory(plRu, "Спорт на Украине", "Спорт України");

            AddFromCategory(plRu, "Природа Украины", "Природа України");
            AddFromCategory(plRu, "Исчезнувшие населённые пункты Украины", "Зниклі населені пункти");            
            
            AddFromCategory(plRu, "Харьковская область", "Харківська область");
            AddFromCategory(plRu, "Донецкая область", "Донецька область");
            AddFromCategory(plRu, "Луганская область", "Луганська область");
                        
            AddFromCategory(plRu, "Волынская область", "Волинська область");            
            AddFromCategory(plRu, "Киевская область", "Київська область");
            

            
            AddFromCategory(plRu, "История Украины", "Історія України", true); // too long ... 1200 //nas punkty

            //AddFromCategory(plRu, "Культура Украины", "Культура України", true); // too long...           
            

            //AddFromCategory(plRu, "География Украины", "Географія України", true); // Населені пункти все заб*ють.

            AddFromCategory(plRu, "Наука на Украине", "Наука України", true);

            AddFromCategory(plRu, "Геология Украины", "Геологія", true);
            */

            //a few articles
            AddFromCategory(plRu, "Черкасская область", "Черкаська область"); //1 page           
            AddFromCategory(plRu, "Днепропетровская область", "Дніпропетровська область"); //1 page           
            AddFromCategory(plRu, "Львовская область", "Львівська область"); //0 pages
            AddFromCategory(plRu, "Тернопольская область", "Тернопільська область"); //0 pages
            AddFromCategory(plRu, "Ивано-Франковская область", "Івано-Франківська область"); //0 pages

            AddFromCategory(plRu, "Черноморский флот", "Флот"); // a few articles
            AddFromCategory(plRu, "Закарпатская область", "Закарпаття"); // a few articles
            AddFromCategory(plRu, "Запорожская область", "Запорізька область");
            AddFromCategory(plRu, "Житомирская область", "Житомирська область");
            AddFromCategory(plRu, "Сражения по войнам", "Битви");
        }

    }

    class NoUkInterwikisPublisherFromPl : NoUkInterwikisPublisher
    {
        public NoUkInterwikisPublisherFromPl(NoUkInterwikisInPl nui)
            : base(nui)
        {
            mainPageNamePart = @"Вікіпедія:Статті плвікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті плвікі про Україну без українських інтервік|{0}";
        }

        public override void PublishCategories(PageList plRu)
        {
            /*
            AddFromCategory(plRu, "Музыка Украины", "Музика України");
            AddFromCategory(plRu, "Авиация Украины", "Авіація України");
             */ 
            
        }
    }

    class NoUkInterwikisPublisherFromFr : NoUkInterwikisPublisher
    {
        public NoUkInterwikisPublisherFromFr(NoUkInterwikisInFr nui)
            : base(nui)
        {
            
            mainPageNamePart = @"Вікіпедія:Статті фрвікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті фрвікі про Україну без українських інтервік|{0}";
        }

        public override void PublishCategories(PageList plRu)
        {
            /*
            AddFromCategory(plRu, "Музыка Украины", "Музика України");
            AddFromCategory(plRu, "Авиация Украины", "Авіація України");
             */

        }

    }

    class NoUkInterwikisPublisherFromEs : NoUkInterwikisPublisher
    {
        public NoUkInterwikisPublisherFromEs(NoUkInterwikisInEs nui)
            : base(nui)
        {

            mainPageNamePart = @"Вікіпедія:Статті есвікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті есвікі про Україну без українських інтервік|{0}";
        }

        public override void PublishCategories(PageList plRu)
        {            
            //AddFromCategory(plRu, "Музыка Украины", "Музика України");
        }
    }

    class NoUkInterwikisPublisherFromPt : NoUkInterwikisPublisher
    {
        public NoUkInterwikisPublisherFromPt(NoUkInterwikisInPt nui)
            : base(nui)
        {

            mainPageNamePart = @"Вікіпедія:Статті птвікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті птвікі про Україну без українських інтервік|{0}";
        }

        public override void PublishCategories(PageList plRu)
        {
            //AddFromCategory(plRu, "Музыка Украины", "Музика України");
        }
    }

    class NoUkInterwikisPublisherFromDe : NoUkInterwikisPublisher
    {
        public NoUkInterwikisPublisherFromDe(NoUkInterwikisInDe nui)
            : base(nui)
        {
            mainPageNamePart = @"Вікіпедія:Статті девікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті девікі про Україну без українських інтервік|{0}";
        }

        public override void PublishCategories(PageList plRu)
        {
            //AddFromCategory(plRu, "Музыка Украины", "Музика України");
        }
    }

    class NoUkInterwikisPublisherFromEn : NoUkInterwikisPublisher
    {
        public NoUkInterwikisPublisherFromEn(NoUkInterwikisInEn nui)
            : base(nui)
        {
            mainPageNamePart = @"Вікіпедія:Статті енвікі про Україну без українських інтервік/";
            templateName = @"Шаблон:Статті енвікі про Україну без українських інтервік|{0}";
        }

        public override void PublishCategories(PageList plRu)
        {
            /*
            AddFromCategory(plRu, "Музыка Украины", "Музика України");
            AddFromCategory(plRu, "Авиация Украины", "Авіація України");
             */

        }

    }
}
