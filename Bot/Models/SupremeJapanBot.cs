using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Bot.Helpers;
using Bot.Services;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;

namespace Bot.Models
{
    public class SupremeJapanBot: SupremeUSABot
    {
        #region Constructors

        public SupremeJapanBot(Release release) : base(release)
        {
            m_selectSizeId = "size";
            m_sizeFieldName = "size";
            m_styleFieldName = "style";
            m_tohruId = "8c2a7a10-2dfd-4fcf-a845-8a7b59f57b49";
            m_tohruIdPath = "https://telejp.supremenewyork.com/id";
            m_tohruCommitPath = "https://telejp.supremenewyork.com/commit";
            m_tohruTrackInfo = @"
pooky¥¡æ­ñ+DOM""DOM*

resize¢¢æ­ñ+*

resize¤æ­ñ+*

	mouseover¾Óæ­ñ+""BODY*

	mouseoverËÓæ­ñ+""BODY*

	mouseoverÖæ­ñ+""P*
.
	mouseover¯Öæ­ñ+credit_card_type""SELECT*

	mouseoverÀÖæ­ñ+""DIV*
'
	mouseoverÒÖæ­ñ+card_details""DIV*
)
	mouseoverãÖæ­ñ+order_email""INPUT*0
'
	mouseoveròÖæ­ñ+	order_tel""INPUT*6
 
	mouseoverÚæ­ñ+""FIELDSET*
)
	mouseoverÚæ­ñ+order_email""INPUT*0
 
	mouseover§Ûæ­ñ+""FIELDSET*
3
	mouseoverÚÛæ­ñ+credit_card_last_name""INPUT*0

focusíÝæ­ñ+*
/
clickÒÞæ­ñ+credit_card_last_name""INPUT*0
4
	mouseover²àæ­ñ+credit_card_first_name""INPUT*0

	mouseoverèàæ­ñ+""LABEL*
1
keydownûç­ñ+credit_card_last_name""INPUT*0
/
keyupûç­ñ+credit_card_last_name""INPUT*0
1
keydownç­ñ+credit_card_last_name""INPUT*0
1
keydownÊç­ñ+credit_card_last_name""INPUT*1
1
keydown¬ç­ñ+credit_card_last_name""INPUT*2
/
keyupÊç­ñ+credit_card_last_name""INPUT*3
/
keyupñç­ñ+credit_card_last_name""INPUT*3
/
keyupÕç­ñ+credit_card_last_name""INPUT*3
1
keydownýç­ñ+credit_card_last_name""INPUT*3
/
keyup²ç­ñ+credit_card_last_name""INPUT*2
1
keydownç­ñ+credit_card_last_name""INPUT*2
/
keyupÃç­ñ+credit_card_last_name""INPUT*1
1
keydown£ç­ñ+credit_card_last_name""INPUT*1
/
keyupÚç­ñ+credit_card_last_name""INPUT*0
1
keydownëç­ñ+credit_card_last_name""INPUT*0
1
keydownòç­ñ+credit_card_last_name""INPUT*0
/
keyupÚç­ñ+credit_card_last_name""INPUT*0
/
keyupç­ñ+credit_card_last_name""INPUT*0
1
keydownüç­ñ+credit_card_last_name""INPUT*0
/
keyupÑç­ñ+credit_card_last_name""INPUT*0
1
keydown³ç­ñ+credit_card_last_name""INPUT*0
1
keydownÂç­ñ+credit_card_last_name""INPUT*0
/
keyup²ç­ñ+credit_card_last_name""INPUT*0
/
keyupõç­ñ+credit_card_last_name""INPUT*0
1
keydownåç­ñ+credit_card_last_name""INPUT*0
1
keydownç­ñ+credit_card_last_name""INPUT*0
1
keydown·ç­ñ+credit_card_last_name""INPUT*1
/
keyupéç­ñ+credit_card_last_name""INPUT*2
/
keyupúç­ñ+credit_card_last_name""INPUT*2
1
keydownóç­ñ+credit_card_last_name""INPUT*2
1
keydownç­ñ+credit_card_last_name""INPUT*3
/
keyup«ç­ñ+credit_card_last_name""INPUT*4
/
keyupáç­ñ+credit_card_last_name""INPUT*4
1
keydownûç­ñ+credit_card_last_name""INPUT*4
/
keyupç­ñ+credit_card_last_name""INPUT*5
/
keyup²ç­ñ+credit_card_last_name""INPUT*5
1
keydownÓç­ñ+credit_card_last_name""INPUT*5
/
keyupâç­ñ+credit_card_last_name""INPUT*5
1
keydownëç­ñ+credit_card_last_name""INPUT*5
1
keydownôç­ñ+credit_card_last_name""INPUT*6
/
keyupç­ñ+credit_card_last_name""INPUT*7
1
keydownç­ñ+credit_card_last_name""INPUT*7
/
keyup¼ç­ñ+credit_card_last_name""INPUT*8
/
keyupêç­ñ+credit_card_last_name""INPUT*8
1
keydown²ç­ñ+credit_card_last_name""INPUT*8
1
keydownÅç­ñ+credit_card_last_name""INPUT*9
0
keyupç­ñ+credit_card_last_name""INPUT*10
0
keyupç­ñ+credit_card_last_name""INPUT*10
2
keydownç­ñ+credit_card_last_name""INPUT*10
/
keyupÓç­ñ+credit_card_last_name""INPUT*9
1
keydown²ç­ñ+credit_card_last_name""INPUT*9
1
keydown§¢ç­ñ+credit_card_last_name""INPUT*8
1
keydownÉ¢ç­ñ+credit_card_last_name""INPUT*7
1
keydownê¢ç­ñ+credit_card_last_name""INPUT*6
1
keydown£ç­ñ+credit_card_last_name""INPUT*5
1
keydown«£ç­ñ+credit_card_last_name""INPUT*4
1
keydownÌ£ç­ñ+credit_card_last_name""INPUT*3
1
keydownî£ç­ñ+credit_card_last_name""INPUT*2
1
keydown¤ç­ñ+credit_card_last_name""INPUT*1
1
keydown°¤ç­ñ+credit_card_last_name""INPUT*0
1
keydownÒ¤ç­ñ+credit_card_last_name""INPUT*0
1
keydownð¤ç­ñ+credit_card_last_name""INPUT*0
1
keydown¥ç­ñ+credit_card_last_name""INPUT*0
1
keydown´¥ç­ñ+credit_card_last_name""INPUT*0
/
keyupÉ¥ç­ñ+credit_card_last_name""INPUT*0
1
keydown¦ç­ñ+credit_card_last_name""INPUT*0
1
keydownº§ç­ñ+credit_card_last_name""INPUT*0
/
keyupª¨ç­ñ+credit_card_last_name""INPUT*1
/
keyup¹¨ç­ñ+credit_card_last_name""INPUT*1
1
keydownªç­ñ+credit_card_last_name""INPUT*1
1
keydownªªç­ñ+credit_card_last_name""INPUT*2
/
keyupÚªç­ñ+credit_card_last_name""INPUT*3
1
keydownãªç­ñ+credit_card_last_name""INPUT*3
/
keyup«ç­ñ+credit_card_last_name""INPUT*4
/
keyup±«ç­ñ+credit_card_last_name""INPUT*4
1
keydownÂ«ç­ñ+credit_card_last_name""INPUT*4
1
keydownä«ç­ñ+credit_card_last_name""INPUT*5
1
keydown¬ç­ñ+credit_card_last_name""INPUT*6
/
keyup©¬ç­ñ+credit_card_last_name""INPUT*7
/
keyup¼¬ç­ñ+credit_card_last_name""INPUT*7
/
keyupø¬ç­ñ+credit_card_last_name""INPUT*7
1
keydownó­ç­ñ+credit_card_last_name""INPUT*7
/
keyupÑ®ç­ñ+credit_card_last_name""INPUT*8
1
keydown¶´ç­ñ+credit_card_last_name""INPUT*8
0
changeÀ´ç­ñ+credit_card_last_name""INPUT*8
0
keyupµç­ñ+credit_card_first_name""INPUT*0
2
keydownû·ç­ñ+credit_card_first_name""INPUT*0
0
keyup©ºç­ñ+credit_card_first_name""INPUT*0
2
keydownÊºç­ñ+credit_card_first_name""INPUT*0
0
keyup¼¼ç­ñ+credit_card_first_name""INPUT*0
 
	mouseoverÀç­ñ+""FIELDSET*

	mouseover­Àç­ñ+""P*

	mouseoverîÀç­ñ+""P*
4
	mouseoverÁç­ñ+credit_card_first_name""INPUT*0
3
	mouseover¡Áç­ñ+credit_card_last_name""INPUT*8
 
	mouseoverãÁç­ñ+""FIELDSET*
3
	mouseoverÃç­ñ+credit_card_last_name""INPUT*8

	mouseoverÈç­ñ+""DIV*

	mouseoverÈç­ñ+""LABEL*
1
keydownÚËç­ñ+credit_card_last_name""INPUT*8
1
keydownùÍç­ñ+credit_card_last_name""INPUT*8
/
keyupÉÎç­ñ+credit_card_last_name""INPUT*1
/
keyupáÎç­ñ+credit_card_last_name""INPUT*1
1
keydownÛÏç­ñ+credit_card_last_name""INPUT*1
/
keyupÜÐç­ñ+credit_card_last_name""INPUT*2
1
keydownÑç­ñ+credit_card_last_name""INPUT*2
/
keyupíÑç­ñ+credit_card_last_name""INPUT*3
1
keydownêÒç­ñ+credit_card_last_name""INPUT*3
/
keyupÀÓç­ñ+credit_card_last_name""INPUT*4
1
keydown©×ç­ñ+credit_card_last_name""INPUT*4
0
change±×ç­ñ+credit_card_last_name""INPUT*4
0
keyupØç­ñ+credit_card_first_name""INPUT*0
2
keydownªÚç­ñ+credit_card_first_name""INPUT*0
2
keydownÝÛç­ñ+credit_card_first_name""INPUT*0
0
keyupÜç­ñ+credit_card_first_name""INPUT*1
0
keyupâÜç­ñ+credit_card_first_name""INPUT*1
2
keydownªÝç­ñ+credit_card_first_name""INPUT*1
0
keyupÞç­ñ+credit_card_first_name""INPUT*2
2
keydownÚÞç­ñ+credit_card_first_name""INPUT*2
0
keyupÙßç­ñ+credit_card_first_name""INPUT*3
2
keydownªàç­ñ+credit_card_first_name""INPUT*3
0
keyupõàç­ñ+credit_card_first_name""INPUT*4
2
keydownáç­ñ+credit_card_first_name""INPUT*4
2
keydownóáç­ñ+credit_card_first_name""INPUT*5
0
keyupâç­ñ+credit_card_first_name""INPUT*6
0
keyupÁâç­ñ+credit_card_first_name""INPUT*6
2
keydownéãç­ñ+credit_card_first_name""INPUT*6
0
keyupÑäç­ñ+credit_card_first_name""INPUT*7
2
keydown±èç­ñ+credit_card_first_name""INPUT*7
1
change¶èç­ñ+credit_card_first_name""INPUT*7
%
keyupéç­ñ+order_email""INPUT*0

	mouseoverûç­ñ+""DIV*
 
	mouseover©ûç­ñ+""FIELDSET*
)
	mouseover¸ûç­ñ+order_email""INPUT*0
 
	mouseoverÊûç­ñ+""FIELDSET*
1
	mouseoverÛûç­ñ+order_billing_state""SELECT*
0
	mouseoverìûç­ñ+order_billing_city""INPUT*3
 
	mouseoverÄýç­ñ+""FIELDSET*
1
	mouseoverÕýç­ñ+order_billing_state""SELECT*

	mouseoveræýç­ñ+""LABEL*
 
	mouseoverÉþç­ñ+""FIELDSET*

	mouseoverØþç­ñ+""DIV*
'
	mouseoverêþç­ñ+	order_tel""INPUT*6
 
	mouseoverðÿç­ñ+""FIELDSET*
)
	mouseoverÂè­ñ+order_email""INPUT*0
'
keydownèè­ñ+order_email""INPUT*0
%
keyupÓè­ñ+order_email""INPUT*0
'
keydownè­ñ+order_email""INPUT*0
%
keyupöè­ñ+order_email""INPUT*1
'
keydownè­ñ+order_email""INPUT*1
%
keyup÷è­ñ+order_email""INPUT*2
'
keydownè­ñ+order_email""INPUT*2
'
keydownïè­ñ+order_email""INPUT*3
%
keyupè­ñ+order_email""INPUT*4
%
keyupÖè­ñ+order_email""INPUT*4
'
keydownñè­ñ+order_email""INPUT*4
%
keyupÆè­ñ+order_email""INPUT*5
'
keydown¿¡è­ñ+order_email""INPUT*5
'
keydown´¥è­ñ+order_email""INPUT*5
'
keydownÔ¥è­ñ+order_email""INPUT*5
'
keydownö¥è­ñ+order_email""INPUT*5
'
keydown¦è­ñ+order_email""INPUT*5
'
keydown·¦è­ñ+order_email""INPUT*5
'
keydownÚ¦è­ñ+order_email""INPUT*5
'
keydownù¦è­ñ+order_email""INPUT*5
'
keydown§è­ñ+order_email""INPUT*5
'
keydownº§è­ñ+order_email""INPUT*5
'
keydownÜ§è­ñ+order_email""INPUT*5
'
keydown¨è­ñ+order_email""INPUT*5
'
keydown¨è­ñ+order_email""INPUT*5
'
keydownÀ¨è­ñ+order_email""INPUT*5
'
keydownå¨è­ñ+order_email""INPUT*5
'
keydown©è­ñ+order_email""INPUT*5
'
keydown¤©è­ñ+order_email""INPUT*5
'
keydownÆ©è­ñ+order_email""INPUT*5
'
keydownä©è­ñ+order_email""INPUT*5
'
keydownú©è­ñ+order_email""INPUT*5
%
keyupßªè­ñ+order_email""INPUT*6
%
keyupïªè­ñ+order_email""INPUT*6
'
keydown¯­è­ñ+order_email""INPUT*6
%
keyupÿ­è­ñ+order_email""INPUT*7
'
keydown¿®è­ñ+order_email""INPUT*7
'
keydown¯è­ñ+order_email""INPUT*8
%
keyup¯¯è­ñ+order_email""INPUT*9
%
keyupß¯è­ñ+order_email""INPUT*9
'
keydown÷°è­ñ+order_email""INPUT*9
&
keyup¿±è­ñ+order_email""INPUT*10
(
keydownö²è­ñ+order_email""INPUT*10
&
keyup¶³è­ñ+order_email""INPUT*11
(
keydownÐµè­ñ+order_email""INPUT*11
&
keyup¿¶è­ñ+order_email""INPUT*12
(
keydownª·è­ñ+order_email""INPUT*12
&
keyupæ·è­ñ+order_email""INPUT*13
(
keydown÷¸è­ñ+order_email""INPUT*13
&
keyupÆ¹è­ñ+order_email""INPUT*14
(
keydownÿºè­ñ+order_email""INPUT*14
'
change»è­ñ+order_email""INPUT*14
#
keyupî»è­ñ+	order_tel""INPUT*6
%
keydownÊÌè­ñ+	order_tel""INPUT*6
#
keyupÍè­ñ+	order_tel""INPUT*1
%
keydownÐÎè­ñ+	order_tel""INPUT*1
#
keyupÏè­ñ+	order_tel""INPUT*2
%
keydownúÏè­ñ+	order_tel""INPUT*2
#
keyup¶Ðè­ñ+	order_tel""INPUT*3
%
keydownÏÔè­ñ+	order_tel""INPUT*3
#
keyupþÔè­ñ+	order_tel""INPUT*4
%
keydownüÙè­ñ+	order_tel""INPUT*4
#
keyup¶Úè­ñ+	order_tel""INPUT*3
%
keydown¨Þè­ñ+	order_tel""INPUT*3
#
keyupîÞè­ñ+	order_tel""INPUT*4
%
keydownÇåè­ñ+	order_tel""INPUT*4
#
keyupúåè­ñ+	order_tel""INPUT*5
%
keydownÏæè­ñ+	order_tel""INPUT*5
#
keyupçè­ñ+	order_tel""INPUT*6
%
keydown¯éè­ñ+	order_tel""INPUT*6
#
keyupêè­ñ+	order_tel""INPUT*7
%
keydown´îè­ñ+	order_tel""INPUT*7
#
keyupöîè­ñ+	order_tel""INPUT*7
%
keydownÝïè­ñ+	order_tel""INPUT*7
#
keyupðè­ñ+	order_tel""INPUT*7
%
keydownÞðè­ñ+	order_tel""INPUT*7
#
keyupñè­ñ+	order_tel""INPUT*7
%
keydownòè­ñ+	order_tel""INPUT*7
#
keyup¥òè­ñ+	order_tel""INPUT*7
%
keydownÎóè­ñ+	order_tel""INPUT*7
#
keyupôè­ñ+	order_tel""INPUT*8
%
keydownõè­ñ+	order_tel""INPUT*8
#
keyupÖõè­ñ+	order_tel""INPUT*8
%
keydownÎöè­ñ+	order_tel""INPUT*8
#
keyup÷è­ñ+	order_tel""INPUT*8
%
keydownî÷è­ñ+	order_tel""INPUT*8
#
keyup¦øè­ñ+	order_tel""INPUT*8
%
keydownùè­ñ+	order_tel""INPUT*8
#
keyupÆùè­ñ+	order_tel""INPUT*8
%
keydownýúè­ñ+	order_tel""INPUT*8
$
changeûè­ñ+	order_tel""INPUT*8
-
keyupÜûè­ñ+order_billing_state""SELECT*
/
keydownªþè­ñ+order_billing_state""SELECT*
.
change²þè­ñ+order_billing_state""SELECT*
-
keyupéþè­ñ+order_billing_state""SELECT*
/
keydownöé­ñ+order_billing_state""SELECT*
.
changeþé­ñ+order_billing_state""SELECT*
-
keyup­é­ñ+order_billing_state""SELECT*
/
keydown¶é­ñ+order_billing_state""SELECT*
.
change½é­ñ+order_billing_state""SELECT*
-
keyup÷é­ñ+order_billing_state""SELECT*
/
keydown¦é­ñ+order_billing_state""SELECT*
,
keyupÿé­ñ+order_billing_city""INPUT*3
.
keydownïé­ñ+order_billing_city""INPUT*3
.
keydowné­ñ+order_billing_city""INPUT*3
,
keyup½é­ñ+order_billing_city""INPUT*1
,
keyupíé­ñ+order_billing_city""INPUT*1
.
keydowné­ñ+order_billing_city""INPUT*1
.
keydownÝé­ñ+order_billing_city""INPUT*2
,
keyupé­ñ+order_billing_city""INPUT*3
.
keydownþé­ñ+order_billing_city""INPUT*3
,
keyupæé­ñ+order_billing_city""INPUT*4
,
keyupé­ñ+order_billing_city""INPUT*4
.
keydowné­ñ+order_billing_city""INPUT*4
,
keyupé­ñ+order_billing_city""INPUT*5
.
keydown¦é­ñ+order_billing_city""INPUT*5
,
keyupµé­ñ+order_billing_city""INPUT*6
.
keydownÎ é­ñ+order_billing_city""INPUT*6
-
changeÒ é­ñ+order_billing_city""INPUT*6
/
keyupÅ¡é­ñ+order_billing_address""INPUT*4
1
keydown¾¥é­ñ+order_billing_address""INPUT*4
1
keydownæ§é­ñ+order_billing_address""INPUT*4
/
keyup¨é­ñ+order_billing_address""INPUT*1
/
keyup­¨é­ñ+order_billing_address""INPUT*1
1
keydown©é­ñ+order_billing_address""INPUT*1
1
keydown½ªé­ñ+order_billing_address""INPUT*2
/
keyupØªé­ñ+order_billing_address""INPUT*3
1
keydown¯«é­ñ+order_billing_address""INPUT*3
/
keyup¼«é­ñ+order_billing_address""INPUT*4
/
keyupÎ¬é­ñ+order_billing_address""INPUT*4
1
keydown½­é­ñ+order_billing_address""INPUT*4
/
keyup»®é­ñ+order_billing_address""INPUT*5
1
keydownÕ¯é­ñ+order_billing_address""INPUT*5
0
changeÚ¯é­ñ+order_billing_address""INPUT*5
+
keyupÅ°é­ñ+order_billing_zip""INPUT*5
-
keydownÖ½é­ñ+order_billing_zip""INPUT*5
-
keydown¼Àé­ñ+order_billing_zip""INPUT*5
+
keyupÁé­ñ+order_billing_zip""INPUT*1
+
keyupÁé­ñ+order_billing_zip""INPUT*1
-
keydownÂé­ñ+order_billing_zip""INPUT*1
+
keyup×Âé­ñ+order_billing_zip""INPUT*2
-
keydownÅÉé­ñ+order_billing_zip""INPUT*2
+
keyupÊé­ñ+order_billing_zip""INPUT*3
-
keydownìÌé­ñ+order_billing_zip""INPUT*3
-
keydownÍÍé­ñ+order_billing_zip""INPUT*4
+
keyupäÍé­ñ+order_billing_zip""INPUT*5
+
keyupÓÎé­ñ+order_billing_zip""INPUT*5
-
keydown­Ùé­ñ+order_billing_zip""INPUT*5
,
change±Ùé­ñ+order_billing_zip""INPUT*5
'
keyupÚé­ñ+
store_address""INPUT*1

	mouseoverµÜé­ñ+""LABEL*

	mouseoverÆÜé­ñ+""LABEL*

	mouseoverÖÜé­ñ+""BODY*
)
keydowníÝé­ñ+
store_address""INPUT*1
*
keyupÓÞé­ñ+credit_card_type""SELECT*

	mouseoveráÞé­ñ+""P*
 
	mouseoverßé­ñ+""FIELDSET*
3
	mouseoverßé­ñ+credit_card_last_name""INPUT*4
4
	mouseover³ßé­ñ+credit_card_first_name""INPUT*7
 
	mouseoverÔßé­ñ+""FIELDSET*

	mouseoveråßé­ñ+""DIV*
 
	mouseoverëàé­ñ+""FIELDSET*

	mouseoverûàé­ñ+""LABEL*

	mouseoveráé­ñ+""DIV*
.
	mouseover®áé­ñ+credit_card_type""SELECT*
!
	mouseover·ãé­ñ+cnb""INPUT*0

click§åé­ñ+cnb""INPUT*19
 
	mouseover½çé­ñ+""FIELDSET*
.
	mouseoverÍçé­ñ+credit_card_type""SELECT*

	mouseoverÜçé­ñ+""P*

	mouseoverïçé­ñ+""B*

	mouseoverèé­ñ+""B*

	mouseoverèé­ñ+""A*/index
$
	mouseover¡èé­ñ+header""HEADER*

focusôé­ñ+*
$
	mouseover¨öé­ñ+header""HEADER*

	mouseover»÷é­ñ+""A*/index

	mouseoverì÷é­ñ+""HGROUP*

	mouseoverý÷é­ñ+""TIME*

	mouseoverøé­ñ+""BODY*

	mouseoverÀøé­ñ+""B*

	mouseoverÑøé­ñ+""B*
&
	mouseoverâøé­ñ+number_v""INPUT*0
 
	mouseoveròøé­ñ+""FIELDSET*
.
	mouseoverùé­ñ+credit_card_type""SELECT*

	mouseoverçùé­ñ+""P*
&
	mouseover÷ùé­ñ+number_v""INPUT*0
 
keydown±þé­ñ+cnb""INPUT*19

keyupÿé­ñ+cnb""INPUT*19
 
keydownÿÿé­ñ+cnb""INPUT*19

keyup½ê­ñ+cnb""INPUT*19
 
keydownÇê­ñ+cnb""INPUT*19

keyupê­ñ+cnb""INPUT*19
 
keydownê­ñ+cnb""INPUT*19

keyupÆê­ñ+cnb""INPUT*19
 
keydownöê­ñ+cnb""INPUT*19

keyupÅê­ñ+cnb""INPUT*19
 
keydownïê­ñ+cnb""INPUT*19

keyupÅê­ñ+cnb""INPUT*19
 
keydown¯ê­ñ+cnb""INPUT*19
 
keydown¶ê­ñ+cnb""INPUT*19

keyupþê­ñ+cnb""INPUT*19

keyupê­ñ+cnb""INPUT*19
 
keydownê­ñ+cnb""INPUT*19

keyupÑê­ñ+cnb""INPUT*19
 
keydownîê­ñ+cnb""INPUT*19

keyup½ê­ñ+cnb""INPUT*19
 
keydown¿ê­ñ+cnb""INPUT*19

keyupê­ñ+cnb""INPUT*19
 
keydownïê­ñ+cnb""INPUT*19

keyupÖê­ñ+cnb""INPUT*19
 
keydownÈê­ñ+cnb""INPUT*19

keyup¤ê­ñ+cnb""INPUT*19
 
keydownØ£ê­ñ+cnb""INPUT*19

keyup¡¤ê­ñ+cnb""INPUT*19
 
keydownö¥ê­ñ+cnb""INPUT*19

keyupô¦ê­ñ+cnb""INPUT*19
 
keydownÞ§ê­ñ+cnb""INPUT*19

keyupÅ¨ê­ñ+cnb""INPUT*19
 
keydown¯´ê­ñ+cnb""INPUT*19

keyupµê­ñ+cnb""INPUT*19
 
keydownã¶ê­ñ+cnb""INPUT*19

keyupÁ·ê­ñ+cnb""INPUT*19
 
keydownñ»ê­ñ+cnb""INPUT*19

keyup¼ê­ñ+cnb""INPUT*19
 
keydown½ê­ñ+cnb""INPUT*19

keyupÆ½ê­ñ+cnb""INPUT*19
 
keydown´Çê­ñ+cnb""INPUT*19
 
keydownÈê­ñ+cnb""INPUT*19

keyupÈê­ñ+cnb""INPUT*19

keyupÕÈê­ñ+cnb""INPUT*19

	mouseoverèËê­ñ+""B*
&
	mouseoverÍê­ñ+number_v""INPUT*0
 
	mouseover¡Íê­ñ+""FIELDSET*

	mouseover°Íê­ñ+""P*
 
	mouseoverÂÍê­ñ+""FIELDSET*
.
	mouseoverÒÍê­ñ+credit_card_type""SELECT*
 
	mouseoverõÍê­ñ+""FIELDSET*
""
	mouseoverÎê­ñ+cnb""INPUT*19
'
	mouseover´Îê­ñ+card_details""DIV*
.
	mouseoverÇÎê­ñ+credit_card_year""SELECT*
'
	mouseoverçÎê­ñ+card_details""DIV*
$
	mouseover÷Îê­ñ+	cart-vval""DIV*
'
	mouseoverÐê­ñ+card_details""DIV*
.
	mouseover¯Ðê­ñ+credit_card_year""SELECT*
*
clickÓê­ñ+credit_card_year""SELECT*
+
change§Öê­ñ+credit_card_year""SELECT*
*
click©Öê­ñ+credit_card_year""SELECT*

	mouseover¼Öê­ñ+""DIV*
&
	mouseoverØê­ñ+cart-totals""DIV*

	mouseover¦Øê­ñ+""DIV*
&
	mouseoverÿÙê­ñ+cart-totals""DIV*
$
	mouseoverÚê­ñ+subtotal""SPAN*
 
	mouseoverÛê­ñ+""FIELDSET*
$
	mouseover¨Ûê­ñ+	cart-vval""DIV*
""
	mouseoveréÛê­ñ+vval""INPUT*0

click¸Ýê­ñ+vval""INPUT*0
 
keydownÜâê­ñ+vval""INPUT*0

keyup³ãê­ñ+vval""INPUT*1
 
keydownäê­ñ+vval""INPUT*1
 
keydownåê­ñ+vval""INPUT*2

keyupåê­ñ+vval""INPUT*3

keyupæê­ñ+vval""INPUT*3

	mouseoveréê­ñ+""DIV*
 
	mouseover®éê­ñ+""FIELDSET*

	mouseover¿éê­ñ+""DIV*
0
	mouseoverÐéê­ñ+order_billing_city""INPUT*6

	mouseoveràéê­ñ+""DIV*

	mouseoverñéê­ñ+""SPAN*

	mouseoverêê­ñ+""STRONG*
 
	mouseover£êê­ñ+""FIELDSET*
A
	mouseover³êê­ñ+""A*(http://www.supremenewyork.com/shop/terms
""
	mouseoverÅêê­ñ+cart-cc""DIV*

	mouseoverîê­ñ+""LABEL*
)
	mouseoverÈîê­ñ+order_terms""INPUT*1

	mouseoverîñê­ñ+""LABEL*
)
	mouseoverêúê­ñ+order_terms""INPUT*1

changeë­ñ+vval""INPUT*3
%
clickÐë­ñ+order_terms""INPUT*1
&
changeÓë­ñ+order_terms""INPUT*1
""
	mouseover¦ë­ñ+cart-cc""DIV*
&
	mouseoveröë­ñ+cart-footer""DIV*

	mouseoverë­ñ+""INPUT*4

clickéë­ñ+""INPUT*4

	mouseoveràë­ñ+""DIV*

	mouseoverîë­ñ+""DIV*

	mouseoverÿë­ñ+""IFRAME*

	mouseoverªë­ñ+""IFRAME*¿Óæ­ñ+ÍÝ Ñ(¶ËÓæ­ñ+Æî Ñ(¶ÕÓæ­ñ+¾ÿ Ñ(¶áÓæ­ñ+²ª Ñ(¶óÓæ­ñ+©Á Ñ(¶ÝÕæ­ñ+¨Ã Ñ(¶íÕæ­ñ+Í Ñ(¶þÕæ­ñ+ÌÞ Ñ(¶Öæ­ñ+ì Ñ(¶Öæ­ñ+ñò Ñ(¶°Öæ­ñ+ Ñ(¶ÀÖæ­ñ+Ð Ñ(¶ÒÖæ­ñ+² Ñ(¶ãÖæ­ñ+ÐÂ Ñ(¶òÖæ­ñ+¢Ï Ñ(¶×æ­ñ+Õ Ñ(¶×æ­ñ+ï× Ñ(¶¤×æ­ñ+àÚ Ñ(¶µ×æ­ñ+ÌÜ Ñ(¶Æ×æ­ñ+ÀÞ Ñ(¶Ö×æ­ñ+ºß Ñ(¶æ×æ­ñ+¸ß Ñ(¶¥Øæ­ñ+·ß Ñ(¶¬Øæ­ñ+µß Ñ(¶»Øæ­ñ+²Ý Ñ(¶ÊØæ­ñ+®Ù Ñ(¶ÛØæ­ñ+­Ø Ñ(¶ÌÙæ­ñ+­Õ Ñ(¶ÜÙæ­ñ+­Ó Ñ(¶äÙæ­ñ+­Ò Ñ(¶òÙæ­ñ+­Ï Ñ(¶Úæ­ñ+­Ê Ñ(¶Úæ­ñ+¬Å Ñ(¶£Úæ­ñ+«Ä Ñ(¶³Úæ­ñ+«Ã Ñ(¶ÆÚæ­ñ+«Á Ñ(¶ÔÚæ­ñ+«À Ñ(¶æÚæ­ñ+«¼ Ñ(¶õÚæ­ñ+«º Ñ(¶Ûæ­ñ+¬µ Ñ(¶Ûæ­ñ+­² Ñ(¶§Ûæ­ñ+®® Ñ(¶¹Ûæ­ñ+®­ Ñ(¶ÉÛæ­ñ+®« Ñ(¶ÚÛæ­ñ+¯© Ñ(¶ëÛæ­ñ+°§ Ñ(¶ûÛæ­ñ+±¦ Ñ(¶ÕÜæ­ñ+±¥ Ñ(¶ßÜæ­ñ+±£ Ñ(¶µßæ­ñ+´¢ Ñ(¶Äßæ­ñ+µ¢ Ñ(¶Îßæ­ñ+¶¢ Ñ(¶àßæ­ñ+º¢ Ñ(¶ïßæ­ñ+¿¢ Ñ(¶àæ­ñ+É¡ Ñ(¶àæ­ñ+Ú Ñ(¶¡àæ­ñ+ó Ñ(¶³àæ­ñ+ Ñ(¶Ãàæ­ñ+® Ñ(¶Óàæ­ñ+Ò Ñ(¶èàæ­ñ+ Ñ(¶õàæ­ñ+ Ñ(¶áæ­ñ+ª Ñ(¶áæ­ñ+®ÿ Ñ(¶Àç­ñ+¢û Ñ(¶Àç­ñ+¢ù Ñ(¶­Àç­ñ+¨õ Ñ(¶ÃÀç­ñ+¥ô Ñ(¶ÍÀç­ñ+ô Ñ(¶ÞÀç­ñ+ûõ Ñ(¶îÀç­ñ+Õÿ Ñ(¶ÿÀç­ñ+µ Ñ(¶Áç­ñ+ Ñ(¶¡Áç­ñ+ö Ñ(¶±Áç­ñ+Ø¢ Ñ(¶ÂÁç­ñ+Â¥ Ñ(¶ÒÁç­ñ+°© Ñ(¶ãÁç­ñ+¥® Ñ(¶»Âç­ñ+¦® Ñ(¶ÈÂç­ñ+§® Ñ(¶ØÂç­ñ+«¬ Ñ(¶éÂç­ñ+¯¬ Ñ(¶ùÂç­ñ+¶ª Ñ(¶Ãç­ñ+¼© Ñ(¶Ãç­ñ+À§ Ñ(¶ÃÃç­ñ+Ã§ Ñ(¶ÌÃç­ñ+Å¦ Ñ(¶ÜÃç­ñ+É¤ Ñ(¶íÃç­ñ+Ì¡ Ñ(¶ÿÃç­ñ+Î  Ñ(¶Äç­ñ+Ï Ñ(¶ëÄç­ñ+Ð Ñ(¶óÄç­ñ+Ó Ñ(¶Åç­ñ+× Ñ(¶Åç­ñ+Ù Ñ(¶¼Æç­ñ+Ú Ñ(¶´Çç­ñ+× Ñ(¶ÃÇç­ñ+Ô Ñ(¶ÐÇç­ñ+É Ñ(¶ëÇç­ñ+« Ñ(¶ôÇç­ñ+¤ Ñ(¶Èç­ñ+ Ñ(¶Èç­ñ+ Ñ(¶¤Èç­ñ+ Ñ(¶¶Èç­ñ+ Ñ(¶­Éç­ñ+ Ñ(¶¼Éç­ñ+ÿ Ñ(¶Óç­ñ+þ Ñ(¶ûç­ñ+ Ñ(¶ûç­ñ+ Ñ(¶©ûç­ñ+³® Ñ(¶¸ûç­ñ+ÇÀ Ñ(¶Êûç­ñ+àæ Ñ(¶Ûûç­ñ+óü Ñ(¶ìûç­ñ+ Ñ(¶üûç­ñ+ Ñ(¶ýç­ñ+ Ñ(¶ýç­ñ+ Ñ(¶£ýç­ñ+ò Ñ(¶³ýç­ñ+Ô Ñ(¶Äýç­ñ+¸ Ñ(¶Õýç­ñ+ Ñ(¶æýç­ñ+ÿ Ñ(¶öýç­ñ+ü Ñ(¶§þç­ñ+û Ñ(¶¸þç­ñ+õ Ñ(¶Éþç­ñ+é Ñ(¶Øþç­ñ+ã Ñ(¶êþç­ñ+à Ñ(¶úþç­ñ+ Þ Ñ(¶ÿç­ñ+¥Ü Ñ(¶ÿç­ñ+ªÙ Ñ(¶¬ÿç­ñ+¯× Ñ(¶½ÿç­ñ+²Ô Ñ(¶Îÿç­ñ+³Ò Ñ(¶Þÿç­ñ+¶Î Ñ(¶ðÿç­ñ+·Ë Ñ(¶ÿÿç­ñ+¸É Ñ(¶±è­ñ+¸È Ñ(¶Âè­ñ+¸Ä Ñ(¶Óè­ñ+ºÀ Ñ(¶ãè­ñ+º¿ Ñ(¶âè­ñ+º¾ Ñ(¶ñè­ñ+º¼ Ñ(¶Áè­ñ+»» Ñ(¶Üé­ñ+·» Ñ(¶¥Üé­ñ+©» Ñ(¶µÜé­ñ+ö¸ Ñ(¶ÆÜé­ñ+¶§ Ñ(¶ÖÜé­ñ+x Ñ(¶æÜé­ñ+p Ñ(¶ýÝé­ñ+p Ñ(¶Þé­ñ+t Ñ(¶Þé­ñ+} Ñ(¶¯Þé­ñ+ Ñ(¶ÀÞé­ñ+ Ñ(¶ÐÞé­ñ+ Ñ(¶áÞé­ñ+ª Ñ(¶òÞé­ñ+Í Ñ(¶ßé­ñ+þ Ñ(¶ßé­ñ+¶ Ñ(¶£ßé­ñ+ò  Ñ(¶³ßé­ñ+©£ Ñ(¶Äßé­ñ+Ò£ Ñ(¶Ôßé­ñ+é  Ñ(¶åßé­ñ+ï Ñ(¶¿àé­ñ+ð Ñ(¶Êàé­ñ+ò Ñ(¶Ûàé­ñ+ÿ Ñ(¶ëàé­ñ+ Ñ(¶ûàé­ñ+´ Ñ(¶áé­ñ+Ì Ñ(¶áé­ñ+á Ñ(¶®áé­ñ+ú Ñ(¶¾áé­ñ+ Ñ(¶Ïáé­ñ+ Ñ(¶Þáé­ñ+ Ñ(¶îáé­ñ+ Ñ(¶âé­ñ+ Ñ(¶âé­ñ+ Ñ(¶¡âé­ñ+¡ Ñ(¶±âé­ñ+¥ Ñ(¶Ââé­ñ+¨ Ñ(¶Óâé­ñ+© Ñ(¶äâé­ñ+­ Ñ(¶õâé­ñ+® Ñ(¶ãé­ñ+° Ñ(¶ãé­ñ+± Ñ(¶¦ãé­ñ+³ Ñ(¶·ãé­ñ+· Ñ(¶Çãé­ñ+¸ Ñ(¶Ùãé­ñ+¹¡ Ñ(¶æé­ñ+¸¢ Ñ(¶æé­ñ+·¢ Ñ(¶çé­ñ+µ¡ Ñ(¶¬çé­ñ+³ Ñ(¶½çé­ñ+¬ Ñ(¶Íçé­ñ+  Ñ(¶Ýçé­ñ+ò Ñ(¶ïçé­ñ+Ï® Ñ(¶èé­ñ+ò Ñ(¶èé­ñ+×¼ Ñ(¶¡èé­ñ+ Ñ(¶°èé­ñ+Øz Ñ(¶Àèé­ñ+«b Ñ(¶Ñèé­ñ+Q Ñ(¶âèé­ñ+B Ñ(¶ôèé­ñ+û8 Ñ(¶éé­ñ+ø0 Ñ(¶éé­ñ+÷( Ñ(¶¤éé­ñ+ö! Ñ(¶µéé­ñ+ò Ñ(¶Çéé­ñ+î Ñ(¶Öéé­ñ+ì	 Ñ(¶æéé­ñ+ì Ñ(¶øéé­ñ+ì Ñ(¶¨öé­ñ+ Ñ(¶µöé­ñ+² Ñ(¶Æöé­ñ+Þ+ Ñ(¶Ööé­ñ+B Ñ(¶çöé­ñ+²X Ñ(¶ùöé­ñ+Ñi Ñ(¶÷é­ñ+äy Ñ(¶÷é­ñ+ò Ñ(¶©÷é­ñ+¡ Ñ(¶»÷é­ñ+³ Ñ(¶Ì÷é­ñ+¢Æ Ñ(¶Û÷é­ñ+­Õ Ñ(¶ì÷é­ñ+µã Ñ(¶ý÷é­ñ+¼ï Ñ(¶øé­ñ+Ãý Ñ(¶ øé­ñ+Ê Ñ(¶°øé­ñ+Ò Ñ(¶Àøé­ñ+Õ« Ñ(¶Ñøé­ñ+Úº Ñ(¶âøé­ñ+æÒ Ñ(¶òøé­ñ+ñä Ñ(¶ùé­ñ+þö Ñ(¶ùé­ñ+ Ñ(¶¤ùé­ñ+ Ñ(¶µùé­ñ+ Ñ(¶Æùé­ñ+ Ñ(¶Õùé­ñ+þ Ñ(¶çùé­ñ+ê Ñ(¶÷ùé­ñ+§Ñ Ñ(¶úé­ñ+©Í Ñ(¶ÑËê­ñ+¡Ì Ñ(¶ØËê­ñ+Ê Ñ(¶éËê­ñ+ÿÄ Ñ(¶úËê­ñ+ó½ Ñ(¶Ìê­ñ+î» Ñ(¶ÐÌê­ñ+ð¼ Ñ(¶ÝÌê­ñ+ôÀ Ñ(¶îÌê­ñ+úÆ Ñ(¶þÌê­ñ+ûÈ Ñ(¶Íê­ñ+ÿÏ Ñ(¶¡Íê­ñ+æ Ñ(¶°Íê­ñ+õ Ñ(¶ÂÍê­ñ+ú Ñ(¶ÒÍê­ñ+ÿ Ñ(¶äÍê­ñ+ Ñ(¶õÍê­ñ+ Ñ(¶Îê­ñ+ Ñ(¶Îê­ñ+¦ Ñ(¶¤Îê­ñ+¬ Ñ(¶´Îê­ñ+² Ñ(¶ÇÎê­ñ+» Ñ(¶ÖÎê­ñ+Æ Ñ(¶çÎê­ñ+Ô Ñ(¶÷Îê­ñ+Ø Ñ(¶Ðê­ñ+Õ Ñ(¶Ðê­ñ+Ô Ñ(¶¯Ðê­ñ+Ï Ñ(¶ÀÐê­ñ+Í Ñ(¶ÐÐê­ñ+É Ñ(¶âÐê­ñ+Æ Ñ(¶òÐê­ñ+Â Ñ(¶Ñê­ñ+¤À Ñ(¶Ñê­ñ+©¿ Ñ(¶£Ñê­ñ+­¿ Ñ(¶µÑê­ñ+²¿ Ñ(¶ÄÑê­ñ+·¿ Ñ(¶ÕÑê­ñ+»¿ Ñ(¶çÑê­ñ+¿¿ Ñ(¶÷Ñê­ñ+Ä¿ Ñ(¶Òê­ñ+Ç¿ Ñ(¶Òê­ñ+ËÁ Ñ(¶¯Òê­ñ+ÌÁ Ñ(¶øÒê­ñ+ÌÁ Ñ(¶üÒê­ñ+ÌÁ Ñ(¶Ôê­ñ+ÍÂ Ñ(¶Ôê­ñ+ÍÄ Ñ(¶¢Ôê­ñ+ÌÇ Ñ(¶³Ôê­ñ+ÌÌ Ñ(¶ÃÔê­ñ+ÌÏ Ñ(¶¼Öê­ñ+Çô Ñ(¶À×ê­ñ+Çö Ñ(¶Æ×ê­ñ+Æø Ñ(¶Ó×ê­ñ+Åù Ñ(¶ã×ê­ñ+Äû Ñ(¶ô×ê­ñ+Ä Ñ(¶Øê­ñ+Â Ñ(¶Øê­ñ+¿ Ñ(¶¦Øê­ñ+¾ Ñ(¶·Øê­ñ+½ Ñ(¶éØê­ñ+» Ñ(¶ºÙê­ñ+º Ñ(¶ÆÙê­ñ+¹ Ñ(¶ÏÙê­ñ+· Ñ(¶ÝÙê­ñ+¶ Ñ(¶îÙê­ñ+° Ñ(¶ÿÙê­ñ+ª Ñ(¶Úê­ñ+¤ Ñ(¶ Úê­ñ+  Ñ(¶±Úê­ñ+þ Ñ(¶ÂÚê­ñ+û Ñ(¶ÕÚê­ñ+ú Ñ(¶ãÚê­ñ+ø Ñ(¶ôÚê­ñ+õ Ñ(¶Ûê­ñ+ñ Ñ(¶Ûê­ñ+î Ñ(¶¨Ûê­ñ+ë Ñ(¶¶Ûê­ñ+ê Ñ(¶ÈÛê­ñ+è Ñ(¶×Ûê­ñ+æ Ñ(¶éÛê­ñ+ã Ñ(¶úÛê­ñ+â Ñ(¶Üê­ñ+á Ñ(¶Üê­ñ+ß Ñ(¶«Üê­ñ+ÿÞ Ñ(¶ïèê­ñ+üÜ Ñ(¶üèê­ñ+ñÛ Ñ(¶éê­ñ+Ñ× Ñ(¶éê­ñ+¬× Ñ(¶®éê­ñ+éè Ñ(¶¿éê­ñ+È÷ Ñ(¶Ðéê­ñ+Ç Ñ(¶àéê­ñ+Ý Ñ(¶ñéê­ñ+ô© Ñ(¶êê­ñ+´ Ñ(¶êê­ñ+¾ Ñ(¶£êê­ñ+Ë Ñ(¶³êê­ñ+ªÙ Ñ(¶Åêê­ñ+¶è Ñ(¶Õêê­ñ+¿÷ Ñ(¶æêê­ñ+Å Ñ(¶öêê­ñ+Ï Ñ(¶ëê­ñ+Ó Ñ(¶ëê­ñ+Ó  Ñ(¶§ëê­ñ+Ô¢ Ñ(¶¹ëê­ñ+Ô¦ Ñ(¶Èëê­ñ+Õ§ Ñ(¶Úëê­ñ+Õ¨ Ñ(¶þëê­ñ+Õ¥ Ñ(¶ìê­ñ+Ò Ñ(¶ìê­ñ+Ê Ñ(¶­ìê­ñ+Â Ñ(¶¾ìê­ñ+¼ú Ñ(¶Íìê­ñ+¸õ Ñ(¶ßìê­ñ+µñ Ñ(¶ïìê­ñ+²ï Ñ(¶íê­ñ+¯ë Ñ(¶íê­ñ+®é Ñ(¶¡íê­ñ+­æ Ñ(¶²íê­ñ+­å Ñ(¶Óíê­ñ+«ä Ñ(¶äíê­ñ+©ä Ñ(¶ôíê­ñ+£ä Ñ(¶îê­ñ+ä Ñ(¶îê­ñ+ã Ñ(¶§îê­ñ+ÿà Ñ(¶¸îê­ñ+ûß Ñ(¶Èîê­ñ+öÞ Ñ(¶Ùîê­ñ+òÞ Ñ(¶ïê­ñ+ñÞ Ñ(¶ïê­ñ+ðÝ Ñ(¶¬ïê­ñ+ïÛ Ñ(¶½ïê­ñ+îÚ Ñ(¶Îïê­ñ+îÙ Ñ(¶Þïê­ñ+î× Ñ(¶Æðê­ñ+ïÖ Ñ(¶Þðê­ñ+ðÖ Ñ(¶éðê­ñ+òÖ Ñ(¶ôðê­ñ+ôÖ Ñ(¶ñê­ñ+÷Ö Ñ(¶ñê­ñ+úØ Ñ(¶îñê­ñ+üØ Ñ(¶×óê­ñ+ý× Ñ(¶÷øê­ñ+ýÖ Ñ(¶ùê­ñ+ýÕ Ñ(¶úê­ñ+üÔ Ñ(¶êúê­ñ+ûÓ Ñ(¶íüê­ñ+ûÕ Ñ(¶þüê­ñ+ûÖ Ñ(¶ýê­ñ+û× Ñ(¶µýê­ñ+ûØ Ñ(¶ûþê­ñ+úÙ Ñ(¶¿ÿê­ñ+ùÙ Ñ(¶Ðÿê­ñ+÷Û Ñ(¶ë­ñ+öÜ Ñ(¶¦ë­ñ+ùå Ñ(¶³ë­ñ+î Ñ(¶Äë­ñ+¤ Ñ(¶Óë­ñ+Ä Ñ(¶åë­ñ+à¦ Ñ(¶öë­ñ+¼ Ñ(¶ë­ñ+Ç Ñ(¶ë­ñ+¤Ï Ñ(¶§ë­ñ+®Ó Ñ(¶¸ë­ñ+²Õ Ñ(¶Éë­ñ+¶Ö Ñ(¶Ùë­ñ+¸Ö Ñ(¶êë­ñ+»Ö Ñ(¶úë­ñ+½Ö Ñ(¶ë­ñ+¿Ö Ñ(¶ë­ñ+ÀÖ Ñ(¶àë­ñ+¿Ö Ñ(¶îë­ñ+±Ù Ñ(¶	/checkout";
        }

        #endregion

        #region Methods

        protected override StepResult Billing(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            cxt.ResetCheckoutInfo();

            using (HttpRequestMessage checkoutPageRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://{task.Footsite.Domain}{m_checkoutPagePath}"),
                Method = HttpMethod.Get
            })
            {
                checkoutPageRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                checkoutPageRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                checkoutPageRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");

                html = HttpHelper.GetStringSync(checkoutPageRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            cxt.CheckoutTimestamp = TimeHelper.GetUnixTimeStamp();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            HtmlNode checkoutFormNode = doc.GetElementbyId("checkout_form");

            if (checkoutFormNode != null)
            {
                HtmlNode creditCardDetailsNode = doc.GetElementbyId("card_details");

                if (creditCardDetailsNode != null)
                {
                    try
                    {
                        HtmlNodeCollection divs = creditCardDetailsNode.SelectNodes("div");

                        cxt.CreditCardNumberFieldName = divs[0]
                            .SelectSingleNode("input")
                            .GetAttributeValue("name", cxt.CreditCardNumberFieldName);
                        cxt.CreditCardMonthFieldName = divs[1].SelectNodes("select")[0]
                            .GetAttributeValue("name", cxt.CreditCardMonthFieldName);
                        cxt.CreditCardYearFieldName = divs[1].SelectNodes("select")[1]
                            .GetAttributeValue("name", cxt.CreditCardYearFieldName);
                        cxt.CreditCardCVVFieldName = divs[2]
                            .SelectSingleNode("input")
                            .GetAttributeValue("name", cxt.CreditCardCVVFieldName);

                        cxt.CsrfToken = doc.DocumentNode.SelectSingleNode("//meta[@name='csrf-token']")
                            .GetAttributeValue("content", "");
                        //cxt.StoreId = checkoutFormNode.SelectSingleNode("//input[@name='store_credit_id']")
                        //    .GetAttributeValue("value", "");
                        cxt.Utf8Symbol = checkoutFormNode.SelectSingleNode("//input[@name='utf8']")
                            .GetAttributeValue("value", "");
                        cxt.AuthenticityToken = checkoutFormNode.SelectSingleNode("//input[@name='authenticity_token']")
                            .GetAttributeValue("value", "");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Could not parse credit card form. Default field names will be used.");
                    }
                }
                else
                {
                    Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Could not find credit card form. Default field names will be used.");
                }

                try
                {
                    HtmlNode extraFieldNode = doc.GetElementbyId("cart-cc").SelectSingleNode("fieldset").SelectNodes("input").Last();

                    cxt.ExtraField = new KeyValuePair<string, string>(extraFieldNode.GetAttributeValue("name", ""), extraFieldNode.GetAttributeValue("value", ""));
                }
                catch (Exception e)
                {

                }
            }
            else
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get checkout form. Contact with support.");

                res = StepResult.Failed;
            }

            return res;
        }

        protected override StepResult Paying(ICheckoutTaskContext context, CheckoutTask task, CancellationToken cancelToken)
        {
            StepResult res = StepResult.Ok;
            SupremeUSABotTaskContext cxt = context as SupremeUSABotTaskContext;

            string html = null;
            HttpStatusCode? statusCode = null;

            int timeElapsed = TimeHelper.GetUnixTimeStamp() - cxt.CheckoutTimestamp;

            if (timeElapsed * 1000 < task.Footsite.Settings.DelayInCheckout)
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}",
                    $"Delay in checkout {task.Footsite.Settings.DelayInCheckout - timeElapsed * 1000} ms");

                cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(task.Footsite.Settings.DelayInCheckout - timeElapsed * 1000));
                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            }

            task.State = CheckoutTaskState.WaitingCaptcha;
            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Waiting captcha...");

            string token = null;

            while (true)
            {
                ICaptchaHarvesterTask harvesterTask = new CaptchaHarvesterTaskBase(null);

                Release.CaptchaHarvester.GetSolution(harvesterTask);

                WaitHandle.WaitAny(new[] { harvesterTask.SolutionReadyEvent, cancelToken.WaitHandle });

                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;

                RecaptchaSolution solution = harvesterTask.Solution as RecaptchaSolution;

                if (solution.TimeStamp > cxt.CheckoutTimestamp)
                {
                    token = solution.Value;

                    break;
                }

                harvesterTask.SolutionReadyEvent.Dispose();
            }

            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Waiting captcha... done");

            task.State = CheckoutTaskState.Paying;

            using (HttpRequestMessage tohruCommitMessage = new HttpRequestMessage()
            {
                RequestUri = new Uri(m_tohruCommitPath),
                Method = HttpMethod.Post
            })
            {
                tohruCommitMessage.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                tohruCommitMessage.Headers.TryAddWithoutValidation("Accept", "*/*");
                tohruCommitMessage.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                tohruCommitMessage.Headers.TryAddWithoutValidation("Content-Type", "application/octet-stream");
                tohruCommitMessage.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                tohruCommitMessage.Content = new StringContent(m_tohruTrackInfo);

                HttpHelper.GetStringSync(tohruCommitMessage, cxt.Client, out statusCode, cancelToken);
            }

            string billPhoneNumber = task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PhoneNumber.Insert(3, "-").Insert(7, "-");

            using (HttpRequestMessage checkoutRequest = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://{task.Footsite.Domain}{m_checkoutPath}"),
                Method = HttpMethod.Post
            })
            {
                checkoutRequest.Headers.TryAddWithoutValidation("User-Agent", BrowserAgent);
                checkoutRequest.Headers.TryAddWithoutValidation("Accept", "*/*");
                checkoutRequest.Headers.TryAddWithoutValidation("Accept-Encoding", "gzip, deflate");
                checkoutRequest.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
                checkoutRequest.Headers.TryAddWithoutValidation("Refer", "https://www.supremenewyork.com/checkout");
                checkoutRequest.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                checkoutRequest.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.8,en-GB;q=0.6");
                checkoutRequest.Headers.Add("X-CSRF-Token", cxt.CsrfToken);
                checkoutRequest.Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
                {
                    new KeyValuePair<string, string>("store_credit_id", cxt.StoreId),
                    new KeyValuePair<string, string>("same_as_billing_address", "1"),
                    new KeyValuePair<string, string>("credit_card[last_name]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.SecondName),
                    new KeyValuePair<string, string>("credit_card[first_name]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.FirstName),
                    new KeyValuePair<string, string>("order[email]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.Email),
                    new KeyValuePair<string, string>("order[tel]", billPhoneNumber),
                    new KeyValuePair<string, string>("order[billing_address]", $"{task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddress1}, {task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.StreetAddressLine2}"),
                    new KeyValuePair<string, string>("order[billing_city]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.City),
                    new KeyValuePair<string, string>("order[billing_state]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.State?.Abbreviation),
                    new KeyValuePair<string, string>("order[billing_zip]", task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.PostalCode),
                    new KeyValuePair<string, string>("order[billing_country]", ConvertCommonCountryNameToSupremeName(task.Profile.ReleaseCheckoutProfile.CheckoutProfile.BillingAddress.Country.Code)),
                    new KeyValuePair<string, string>("credit_card[type]", ConvertCommonCreditCardTypeToSupremeType(task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.Type.Code) ?? ""),
                    new KeyValuePair<string, string>(cxt.CreditCardNumberFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.Number),
                    new KeyValuePair<string, string>(cxt.CreditCardMonthFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("MM")),
                    new KeyValuePair<string, string>(cxt.CreditCardYearFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.ExpirationDate.Value.ToString("yyyy")),
                    new KeyValuePair<string, string>(cxt.CreditCardCVVFieldName, task.Profile.ReleaseCheckoutProfile.CheckoutProfile.PayCard.CVS),
                    new KeyValuePair<string, string>("order[terms]", "0"),
                    new KeyValuePair<string, string>("order[terms]", "1"),
                    new KeyValuePair<string, string>("g-recaptcha-response", token),
                    new KeyValuePair<string, string>("utf8", cxt.Utf8Symbol),
                    new KeyValuePair<string, string>("authenticity_token", cxt.AuthenticityToken),

                    cxt.ExtraField

                });

                html = HttpHelper.GetStringSync(checkoutRequest, cxt.Client, out statusCode, cancelToken);
            }

            if (cancelToken.IsCancellationRequested) return StepResult.Canceled;
            if ((res = CheckResult(html, statusCode)) != StepResult.Ok) return res;

            JObject checkoutReponseJObject = null;
            string checkoutStatus = null;
            string slug = null;

            try
            {
                checkoutReponseJObject = JObject.Parse(html);
                checkoutStatus = checkoutReponseJObject["status"].Value<string>();
            }
            catch (Exception e)
            {
                Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get checkout response. Contact with support.");
            }

            if (checkoutStatus != null)
            {


                if (checkoutStatus == "queued")
                {
                    try
                    {
                        slug = checkoutReponseJObject["slug"].Value<string>();
                    }
                    catch (Exception e)
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", "Could not get queue id. Contact with support.");
                    }

                    if (slug != null)
                    {
                        cxt.Slug = slug;

                        CheckoutStep queuedCheckout = new CheckoutStep(QueuedCheckout, "Waiting in Queue", CheckoutTaskState.Undefined);
                        CheckoutStep retryCheckout = new CheckoutStep(RetryCheckout, "Checking paying status", CheckoutTaskState.Undefined);
                        TimeSpan executionTime;

                        res = queuedCheckout.Run(cxt, task, TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod), out executionTime, cancelToken);

                        if (res == StepResult.Ok)
                        {
                            checkoutStatus = cxt.QueuedCheckoutResponse["status"].Value<string>();

                            int attempts = 1;

                            while (attempts < 5 && res != StepResult.Canceled && checkoutStatus == "failed")
                            {
                                Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Attempt {attempts} is failed... retry");
                                attempts++;

                                cancelToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod));
                                if (cancelToken.IsCancellationRequested) return StepResult.Canceled;

                                res = retryCheckout.Run(cxt, task,
                                    TimeSpan.FromMilliseconds(task.Footsite.Settings.RetryPeriod),
                                    out executionTime, cancelToken);

                                if (res == StepResult.Ok)
                                {
                                    checkoutStatus = cxt.QueuedCheckoutResponse["status"].Value<string>();
                                }
                            }
                        }
                    }
                }

                if (res == StepResult.Ok)
                {
                    if (checkoutStatus == "failed")
                    {
                        string errorMessage = "";

                        try
                        {
                            foreach (JToken errorJToken in checkoutReponseJObject["errors"])
                            {
                                errorMessage += $"{errorJToken.First().Value<string>()};";
                            }
                        }
                        catch (Exception e)
                        {
                            ;
                        }

                        if (errorMessage != "")
                        {
                            Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got errors: {errorMessage}");
                        }

                        res = StepResult.Failed;
                    }
                    else if (checkoutStatus == "dup")
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}", $"Has got duplication");

                        res = StepResult.Failed;
                    }
                    else if (checkoutReponseJObject.ToString().Contains("Your order has been submitted") || cxt.QueuedCheckoutResponse != null && cxt.QueuedCheckoutResponse.ToString().Contains("Your order has been submitted"))
                    {
                        res = StepResult.Ok;
                    }
                    else
                    {
                        Logger.LogEvent(task.Log, $"TASK {task.Id}",
                            $"Could not resolve checkout status '{checkoutStatus}'. Contact with support.");

                        res = StepResult.Failed;
                    }
                }
            }
            else
            {
                res = StepResult.Failed;
            }

            return res;
        }

        #endregion
    }
}
