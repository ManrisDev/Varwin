

using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class I18n : IEquatable<I18n>
{
    public string en { get => GetL("en"); set => SetL("en", value); }
    public string af { get => GetL("af"); set => SetL("af", value); }
    public string ar { get => GetL("ar"); set => SetL("ar", value); }
    public string ba { get => GetL("ba"); set => SetL("ba", value); }
    public string be { get => GetL("be"); set => SetL("be", value); }
    public string bu { get => GetL("bu"); set => SetL("bu", value); }
    public string ca { get => GetL("ca"); set => SetL("ca", value); }
    public string cn { get => GetL("cn"); set => SetL("cn", value); }
    public string cz { get => GetL("cz"); set => SetL("cz", value); }
    public string da { get => GetL("da"); set => SetL("da", value); }
    public string du { get => GetL("du"); set => SetL("du", value); }
    public string es { get => GetL("es"); set => SetL("es", value); } 
    public string fa { get => GetL("fa"); set => SetL("fa", value); }
    public string fi { get => GetL("fi"); set => SetL("fi", value); }
    public string fr { get => GetL("fr"); set => SetL("fr", value); }
    public string ge { get => GetL("ge"); set => SetL("ge", value); }
    public string gr { get => GetL("gr"); set => SetL("gr", value); }
    public string he { get => GetL("he"); set => SetL("he", value); }
    public string hu { get => GetL("hu"); set => SetL("hu", value); }
    public string ic { get => GetL("ic"); set => SetL("ic", value); }
    public string ind { get => GetL("ind"); set => SetL("ind", value); }
    public string it { get => GetL("it"); set => SetL("it", value); }
    public string jp { get => GetL("jp"); set => SetL("jp", value); }
    public string ko { get => GetL("ko"); set => SetL("ko", value); }
    public string la { get => GetL("la"); set => SetL("la", value); }
    public string li { get => GetL("li"); set => SetL("li", value); }
    public string no { get => GetL("no"); set => SetL("no", value); }
    public string po { get => GetL("po"); set => SetL("po", value); }
    public string pg { get => GetL("pg"); set => SetL("pg", value); }
    public string ro { get => GetL("ro"); set => SetL("ro", value); }
    public string ru { get => GetL("ru"); set => SetL("ru", value); }
    public string se { get => GetL("se"); set => SetL("se", value); }
    public string sk { get => GetL("sk"); set => SetL("sk", value); }
    public string sv { get => GetL("sv"); set => SetL("sv", value); }
    public string sp { get => GetL("sp"); set => SetL("sp", value); }
    public string sw { get => GetL("sw"); set => SetL("sw", value); }
    public string th { get => GetL("th"); set => SetL("th", value); }
    public string tu { get => GetL("tu"); set => SetL("tu", value); }
    public string uk { get => GetL("uk"); set => SetL("uk", value); }
    public string vi { get => GetL("vi"); set => SetL("vi", value); }
    public string cn_s { get => GetL("cn_s"); set => SetL("cn_s", value); }
    public string cn_t { get => GetL("cn_t"); set => SetL("cn_t", value); }
    public string kk { get => GetL("kk"); set => SetL("kk", value); }

    private Dictionary<string, string> _languageDictionary = new Dictionary<string, string>();

    private string GetL(string lang)
    {
        return _languageDictionary.TryGetValue(lang, out var name) ? name : null;
    }
    
    private void SetL(string lang, string name)
    {
        _languageDictionary[lang] = name;
    }

    public bool PartiallyContainsName(string name)
    {
        return _languageDictionary.Values.Any(x => x.ToLowerInvariant().Contains(name.ToLowerInvariant()));
    }
    
    public bool IsEmpty()
    {
        return _languageDictionary.Values.Count == 0;
    }
    
    public bool Equals(I18n other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        
        return string.Equals(en, other.en, StringComparison.Ordinal) && string.Equals(af, other.af, StringComparison.Ordinal) &&
               string.Equals(ar, other.ar, StringComparison.Ordinal) && string.Equals(ba, other.ba, StringComparison.Ordinal) &&
               string.Equals(be, other.be, StringComparison.Ordinal) && string.Equals(bu, other.bu, StringComparison.Ordinal) &&
               string.Equals(ca, other.ca, StringComparison.Ordinal) && string.Equals(cn, other.cn, StringComparison.Ordinal) &&
               string.Equals(cz, other.cz, StringComparison.Ordinal) && string.Equals(da, other.da, StringComparison.Ordinal) &&
               string.Equals(du, other.du, StringComparison.Ordinal) && string.Equals(es, other.es, StringComparison.Ordinal) &&
               string.Equals(fa, other.fa, StringComparison.Ordinal) && string.Equals(fi, other.fi, StringComparison.Ordinal) &&
               string.Equals(fr, other.fr, StringComparison.Ordinal) && string.Equals(ge, other.ge, StringComparison.Ordinal) &&
               string.Equals(gr, other.gr, StringComparison.Ordinal) && string.Equals(he, other.he, StringComparison.Ordinal) &&
               string.Equals(hu, other.hu, StringComparison.Ordinal) && string.Equals(ic, other.ic, StringComparison.Ordinal) &&
               string.Equals(ind, other.ind, StringComparison.Ordinal) && string.Equals(it, other.it, StringComparison.Ordinal) &&
               string.Equals(jp, other.jp, StringComparison.Ordinal) && string.Equals(ko, other.ko, StringComparison.Ordinal) &&
               string.Equals(la, other.la, StringComparison.Ordinal) && string.Equals(li, other.li, StringComparison.Ordinal) &&
               string.Equals(no, other.no, StringComparison.Ordinal) && string.Equals(po, other.po, StringComparison.Ordinal) &&
               string.Equals(pg, other.pg, StringComparison.Ordinal) && string.Equals(ro, other.ro, StringComparison.Ordinal) &&
               string.Equals(ru, other.ru, StringComparison.Ordinal) && string.Equals(se, other.se, StringComparison.Ordinal) &&
               string.Equals(sk, other.sk, StringComparison.Ordinal) && string.Equals(sv, other.sv, StringComparison.Ordinal) &&
               string.Equals(sp, other.sp, StringComparison.Ordinal) && string.Equals(sw, other.sw, StringComparison.Ordinal) &&
               string.Equals(th, other.th, StringComparison.Ordinal) && string.Equals(tu, other.tu, StringComparison.Ordinal) &&
               string.Equals(uk, other.uk, StringComparison.Ordinal) && string.Equals(vi, other.vi, StringComparison.Ordinal) &&
               string.Equals(cn_s, other.cn_s, StringComparison.Ordinal) && string.Equals(cn_t, other.cn_t, StringComparison.Ordinal) &&
               string.Equals(kk, other.kk, StringComparison.Ordinal) && string.Equals(kk, other.kk, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((I18n) obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = (en != null ? en.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (af != null ? af.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ar != null ? ar.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ba != null ? ba.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (be != null ? be.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (bu != null ? bu.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ca != null ? ca.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (cn != null ? cn.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (cz != null ? cz.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (da != null ? da.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (du != null ? du.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (es != null ? es.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (fa != null ? fa.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (fi != null ? fi.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (fr != null ? fr.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ge != null ? ge.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (gr != null ? gr.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (he != null ? he.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (hu != null ? hu.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ic != null ? ic.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ind != null ? ind.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (it != null ? it.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (jp != null ? jp.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ko != null ? ko.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (la != null ? la.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (li != null ? li.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (no != null ? no.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (po != null ? po.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (pg != null ? pg.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ro != null ? ro.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (ru != null ? ru.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (se != null ? se.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (sk != null ? sk.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (sv != null ? sv.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (sp != null ? sp.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (sw != null ? sw.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (th != null ? th.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (tu != null ? tu.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (uk != null ? uk.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (vi != null ? vi.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (cn_s != null ? cn_s.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (cn_t != null ? cn_t.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (kk != null ? kk.GetHashCode() : 0);
            return hashCode;
        }
    }

    public static bool Equals(I18n a, I18n b)
    {
        if (a == null && b == null)
        {
            return true;
        }
        if (a != null)
        {
            return a.Equals(b);
        }
        
        return false;
    }
    
    public override string ToString()
    {
        return en ?? ru;
    }
}